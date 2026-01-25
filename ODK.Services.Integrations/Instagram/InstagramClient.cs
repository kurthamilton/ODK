using System.Text.Json.Nodes;
using ODK.Core.Utils;
using ODK.Services.Integrations.Instagram.Models;
using ODK.Services.Logging;
using ODK.Services.SocialMedia;
using ODK.Services.SocialMedia.Models;

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

        _httpClient = new(() => _httpClientFactory.CreateClient());
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
        var feedResult = await FetchFeed(username);
        if (feedResult.Value == null)
        {
            return new InstagramPostsResult(false);
        }

        if (JsonUtils.Find(
            feedResult.Value,
            x => x.Node is JsonArray && x.PropertyName == "edges") is not JsonArray edges)
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

    private async Task<ServiceResult<JsonNode>> FetchFeed(string username)
    {        
        var client = _httpClient.Value;

        var request = CreateRequest(HttpMethod.Post, _settings.GraphQLUrl, username);

        // If requests start failing at an auth level, reload Instagram in a browser and update the cookie
        // values in config.
        // If requests start failing at a request level, reload Instagram in a browser and find the /graphql/query
        // request containing the payload that looks like a request for posts and contains an array of ~12 elements.
        // Make sure the structure of the "variables" property below matches exactly, and make sure the "doc_id" property
        // matches.
        // Otherwise, check the response parsing needs updating
        request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["doc_id"] = _settings.PostsGraphQlDocId,
            ["variables"] = JsonUtils.Serialize(new
            {
                data = new
                {
                    count = 12,
                    include_reel_media_seen_timestamp = true,
                    include_relationship_info = true,
                    latest_besties_reel_media = true,
                    latest_reel_media = true
                },
                username = username
            })
        });

        var response = await client.SendAsync(request);

        var json = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            var message = $"InstagramClient: Error getting feed: {json}";
            await _loggingService.Error(message);
            return ServiceResult<JsonNode>.Failure(message);
        }

        var node = JsonNode.Parse(json);
        if (node == null)
        {
            var message = $"InstagramClient: Error parsing feed JSON: {json}";
            await _loggingService.Error(message);
            return ServiceResult<JsonNode>.Failure(message);
        }
        
        return ServiceResult<JsonNode>.Successful(node);
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string url, string username)
    {
        var request = new HttpRequestMessage(method, url);

        var cookies = string.Join("; ", _settings.Cookies.Select(x => $"{x.Key}={x.Value}"));
        request.Headers.TryAddWithoutValidation("Cookie", cookies);

        foreach (var header in _settings.Headers)
        {
            request.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        var channelUrl = _settings.ChannelUrl.Replace("{username}", username);
        var baseUrl = UrlUtils.BaseUrl(channelUrl);

        request.Headers.TryAddWithoutValidation("Origin", baseUrl);
        request.Headers.TryAddWithoutValidation("Referer", channelUrl);

        return request;
    }

    private async Task<InstagramImageResponse> ParseImage(JsonNode node)
    {
        var id = node["pk"]?.GetValue<string>();
        if (string.IsNullOrEmpty(id))
        {
            throw new Exception("Error parsing Instagram image: pk not found");
        }

        var candidates = ((node["image_versions2"]?["candidates"]) as JsonArray)
            ?.Select(x => new
            {
                Height = x?["height"]?.GetValue<int>(),
                Url = x?["url"]?.GetValue<string>(),
                Width = x?["width"]?.GetValue<int>()
            });

        var candidate = candidates
            ?.OrderByDescending(x => x.Height)
            .FirstOrDefault();

        if (string.IsNullOrEmpty(candidate?.Url))
        {
            throw new Exception("Error parsing Instagram image: image_versions2:candidates not found");
        }

        var mediaType = node["media_type"]?.GetValue<int>();
        if (mediaType == null)
        {
            await _loggingService.Warn("Instagram image parsing: media_type not found");
        }

        return new InstagramImageResponse
        {
            Alt = node["accessibility_caption"]?.GetValue<string>(),
            Height = candidate.Height,
            IsVideo = mediaType == 8,
            Shortcode = id,
            Url = candidate.Url,
            Width = candidate.Width
        };
    }

    private async Task<IReadOnlyCollection<InstagramImageResponse>> ParseImages(JsonNode node)
    {
        try
        {
            var images = new List<InstagramImageResponse>();

            var children = node["carousel_media"] as JsonArray;
            if (children == null)
            {
                // the post only contains 1 image
                var image = await ParseImage(node);
                images.Add(image);
                return images;
            }

            foreach (var child in children)
            {                
                if (child == null)
                {
                    continue;
                }

                var image = await ParseImage(child);
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
            var shortcode = node["code"]?.ToString() ?? string.Empty;
            if (string.IsNullOrEmpty(shortcode))
            {
                throw new Exception("Error parsing Instagram post: code not found");
            }

            var unixTimestamp = node["taken_at"]?.GetValue<int>();
            if (unixTimestamp == null)
            {
                throw new Exception("Error parsing Instagram post: taken_at not found");
            }

            var date = DateUtils.FromUnixEpochTimestamp(unixTimestamp.Value);
            
            var caption = node["caption"]?["text"]?.GetValue<string>();

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