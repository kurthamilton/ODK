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

        _httpClient = new(() =>
        {
            var httpClient = _httpClientFactory.CreateClient();

            if (!string.IsNullOrEmpty(settings.UserAgent))
            {
                httpClient.DefaultRequestHeaders.Add("User-Agent", settings.UserAgent);
            }

            return httpClient;
        });
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

    public async Task<IReadOnlyCollection<InstagramClientPost>> FetchLatestPosts(string username)
    {
        var feedJson = await FetchFeedJson(username);
        if (string.IsNullOrEmpty(feedJson))
        {
            return [];
        }

        var edges = ParseFeedEdges(feedJson);
        if (edges == null)
        {
            return [];
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

        return posts;
    }

    private async Task<string?> FetchFeedJson(string username)
    {
        var url = _settings.FeedUrl.Replace("{username}", username);

        var response = await _httpClient.Value.GetAsync(url);
        var json = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            await _loggingService.Error($"Error fetching from Instagram feed: {json}");
            return null;
        }

        return json;
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