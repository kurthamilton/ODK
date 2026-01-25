using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using ODK.Core.Utils;
using ODK.Services.Integrations.Instagram.Models;
using ODK.Services.Logging;
using ODK.Services.SocialMedia;
using ODK.Services.SocialMedia.Models;
using PuppeteerSharp;

namespace ODK.Services.Integrations.Instagram;

public class InstagramClient : IInstagramClient
{
    private readonly Lazy<HttpClient> _httpClient;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILoggingService _loggingService;
    private readonly InstagramClientSettings _settings;

    public InstagramClient(
        IHttpClientFactory httpClientFactory,
        ILoggingService loggingService,
        InstagramClientSettings settings)
    {
        _httpClientFactory = httpClientFactory;
        _loggingService = loggingService;
        _settings = settings;

        _httpClient = new(() => _httpClientFactory.CreateClient("InstagramClient"));
    }

    public async Task<InstagramClientImage> FetchImage(InstagramClientImageMetadata metadata)
    {
        var response = await _httpClient.Value.GetAsync(metadata.Url);
        response.EnsureSuccessStatusCode();

        var imageData = await response.Content.ReadAsByteArrayAsync();
        var mimeType = response.Content.Headers.ContentType?.MediaType;

        return new InstagramClientImage
        {
            ImageData = imageData,
            MimeType = mimeType
        };
    }

    public async Task<InstagramPostsResult> FetchLatestPosts(string username)
    {
        var feedResult = await FetchFeed2(username);
        if (string.IsNullOrEmpty(feedResult.Value))
        {
            return new InstagramPostsResult(false);
        }

        var edges = ParseFeedEdges(feedResult.Value);
        if (edges == null)
        {
            return new InstagramPostsResult(false);
        }

        var posts = new List<InstagramClientPost>();

        foreach (var edge in edges)
        {
            var node = edge?["node"];
            if (node == null)
            {
                continue;
            }

            var post = await ParsePost(node);
            if (post == null)
            {
                continue;
            }

            var images = await ParseImages(node);
            if (images.Count == 0)
            {
                continue;
            }

            posts.Add(new InstagramClientPost
            {
                Caption = post.Caption,
                Date = post.DateUtc,
                ExternalId = post.Shortcode,
                Images = images
                    .Select(x => new InstagramClientImageMetadata
                    {
                        Alt = x.Alt,
                        Height = x.Height,
                        ExternalId = x.Shortcode,
                        IsVideo = x.IsVideo,
                        Url = x.Url,
                        Width = x.Width
                    })
                    .ToArray()
            });
        }

        return new InstagramPostsResult(posts);
    }

    private async Task<ServiceResult<string>> FetchFeed1(string username)
    {
        throw new NotImplementedException();
    }

    private async Task<ServiceResult<string>> FetchFeed2(string username)
    {
        var channelUrl = _settings.ChannelUrl.Replace("{username}", username);
        var baseUrl = UrlUtils.BaseUrl(channelUrl);

        var browser = await Puppeteer.LaunchAsync(new LaunchOptions
        {
            Headless = true,
            ExecutablePath = _settings.ChromePath
        });

        var page = await browser.NewPageAsync();

        // --- 4️⃣ Set Instagram cookies ---
        var initialCookies = _settings.Cookies
            .Select(x => new CookieParam
            {
                Name = x.Key,
                Value = x.Value,
                Domain = ".instagram.com",
                Path = "/",
                HttpOnly = true,
                Secure = true
            });

        await page.SetCookieAsync(initialCookies.ToArray());

        // --- 5️⃣ Capture numeric user ID from first GraphQL request ---
        string? userId = null;

        page.RequestFinished += async (sender, e) =>
        {
            if (userId != null) return; // already captured

            var req = e.Request;
            if (req.Url.Contains("/graphql/query") && req.Method.Method == HttpMethod.Post.Method)
            {
                var postData = req.PostData;
                if (!string.IsNullOrEmpty(postData))
                {
                    try
                    {
                        using var doc = JsonDocument.Parse(postData);
                        if (doc.RootElement.TryGetProperty("variables", out var variables))
                        {
                            if (variables.TryGetProperty("id", out var idProp))
                            {
                                userId = idProp.GetString();
                                Console.WriteLine("Captured numeric user ID: " + userId);
                            }
                        }
                    }
                    catch
                    {
                        // ignore parse errors
                    }
                }
            }
        };

        // --- 5️⃣ Navigate to profile page to bootstrap session ---
        await page.GoToAsync(channelUrl, WaitUntilNavigation.Networkidle0);

        // wait until userId is captured or timeout
        int attempts = 0;
        while (userId == null && attempts < 20)
        {
            await Task.Delay(250);
            attempts++;
        }

        if (userId == null)
        {
            var message = "InstagramClient: Failed to capture numeric user ID.";
            await _loggingService.Error(message);
            await browser.CloseAsync();
            return ServiceResult<string>.Failure(message);
        }

        // --- 6️⃣ Extract csrftoken from cookies ---
        var cookies = await page.GetCookiesAsync(baseUrl);
        var csrftokenCookie = Array.Find(cookies, c => c.Name == "csrftoken")?.Value;
        if (csrftokenCookie == null)
        {
            var message = "InstagramClient: Could not find csrftoken cookie.";
            await _loggingService.Error(message);
            await browser.CloseAsync();
            return ServiceResult<string>.Failure(message);
        }

        // --- 7️⃣ Extract numeric user ID dynamically ---
        var userIdJson = await page.EvaluateExpressionAsync<string>(
            "window.__additionalData?.entry_data?.ProfilePage?.[0]?.graphql?.user?.id || null"
        );

        if (userIdJson == null)
        {
            var message = "InstagramClient: Could not extract user ID.";
            await _loggingService.Error(message);
            await browser.CloseAsync();
            return ServiceResult<string>.Failure(message);
        }

        var targetUserId = userIdJson;

        await browser.CloseAsync();

        // --- 8️⃣ POST GraphQL request ---
        var graphqlUrl = "https://www.instagram.com/graphql/query/";

        // Variables for timeline query
        var variables = new
        {
            id = targetUserId,
            first = 12
        };

        // Convert variables to JSON string
        var variablesJson = JsonUtils.Serialize(variables);

        // Payload
        var payload = new MultipartFormDataContent
        {
            { new StringContent(variablesJson, Encoding.UTF8, "application/json"), "variables" },
            // Optional: Include query_id or doc_id if needed, depends on endpoint
        };

        // Add session cookies
        var request = CreateRequest(HttpMethod.Post, graphqlUrl, username);
        request.Content = payload;

        // CSRF header
        request.Headers.Add("X-CSRFToken", csrftokenCookie);

        var response = await _httpClient.Value.SendAsync(request);

        var json = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            var message = $"InstagramClient: Error getting feed: {json}";
            await _loggingService.Error(message);
            return ServiceResult<string>.Failure(message);
        }

        return ServiceResult<string>.Successful(json);
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string url, string username)
    {
        var request = new HttpRequestMessage(method, url);
        foreach (var header in _settings.Headers)
        {
            request.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        request.Headers.Referrer = new Uri(_settings.ChannelUrl.Replace("{username}", username));

        return request;
    }

    private async Task<ServiceResult<string>> FetchFeedMetadata(string username)
    {
        var url = _settings.FeedUrl.Replace("{username}", username);

        var request = CreateRequest(HttpMethod.Get, url, username);

        var response = await _httpClient.Value.SendAsync(request);
        var json = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            await _loggingService.Error($"Error fetching from Instagram feed: {json}");
            return ServiceResult<string>.Failure(json);
        }

        return ServiceResult<string>.Successful(json);
    }

    private JsonArray? ParseFeedEdges(string feedJson)
    {
        try
        {
            var data = JsonNode.Parse(feedJson);
            var edges = data?["data"]?["user"]?["edge_owner_to_timeline_media"]?["edges"] as JsonArray;
            return edges;
        }
        catch
        {
            return null;
        }
    }

    private async Task<InstagramImageResponse> ParseImage(JsonNode node)
    {
        var shortcode = node["shortcode"]?.GetValue<string>();
        if (string.IsNullOrEmpty(shortcode))
        {
            throw new Exception("shortcode not found");
        }

        var url = node["display_url"]?.GetValue<string>();
        if (string.IsNullOrEmpty(url))
        {
            throw new Exception("display_url not found");
        }

        var height = node["dimensions"]?["height"]?.GetValue<int>();
        var width = node["dimensions"]?["width"]?.GetValue<int>();

        var isVideo = node["is_video"]?.GetValue<bool>() ?? false;

        var alt = node["accessibility_caption"]?.GetValue<string>();

        return new InstagramImageResponse
        {
            Alt = alt,
            Height = height,
            IsVideo = isVideo,
            Shortcode = shortcode,
            Url = url,
            Width = width
        };
    }

    private async Task<IReadOnlyCollection<InstagramImageResponse>> ParseImages(JsonNode node)
    {
        try
        {
            var images = new List<InstagramImageResponse>();

            var children = node["edge_sidecar_to_children"]?["edges"] as JsonArray;
            if (children == null)
            {
                // the post only contains 1 image
                var image = await ParseImage(node);
                images.Add(image);
                return images;
            }

            foreach (var child in children)
            {
                var childNode = child?["node"];
                if (childNode == null)
                {
                    throw new Exception("node not found");
                }

                var image = await ParseImage(childNode);

                images.Add(image);
            }

            return images;
        }
        catch (Exception ex)
        {
            await _loggingService.Error($"Error parsing Instagram image: {node}", ex);
            return [];
        }
    }

    private async Task<InstagramPostResponse?> ParsePost(JsonNode node)
    {
        try
        {
            var shortcode = node["shortcode"]?.ToString() ?? string.Empty;
            if (string.IsNullOrEmpty(shortcode))
            {
                throw new Exception("shortcode not found");
            }

            var unixTimestamp = node["taken_at_timestamp"]?.GetValue<int>() ?? 0;
            var date = DateUtils.FromUnixEpochTimestamp(unixTimestamp);
            var caption = (node["edge_media_to_caption"]?["edges"] as JsonArray)?.FirstOrDefault()?["node"]?["text"]?.ToString() ?? string.Empty;

            return new InstagramPostResponse
            {
                Caption = caption,
                DateUtc = date,
                Shortcode = shortcode
            };
        }
        catch (Exception ex)
        {
            await _loggingService.Error($"Error parsing Instagram post: {node}", ex);
            return null;
        }
    }
}