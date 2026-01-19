using System.Text.Json.Nodes;
using ODK.Core.Utils;
using ODK.Core.Web;
using ODK.Services.Integrations.Instagram.Models;
using ODK.Services.Logging;
using ODK.Services.SocialMedia;
using ODK.Services.SocialMedia.Models;

namespace ODK.Services.Integrations.Instagram;

public class InstagramClient : IInstagramClient
{
    private readonly HttpClient _httpClient;
    private readonly ILoggingService _loggingService;

    public InstagramClient(
        IHttpClientFactory httpClientFactory,
        ILoggingService loggingService,
        InstagramClientSettings settings)
    {
        _httpClient = httpClientFactory.CreateClient();

        if (!string.IsNullOrEmpty(settings.UserAgent))
        {
            _httpClient.DefaultRequestHeaders.Add("User-Agent", settings.UserAgent);
        }        

        _loggingService = loggingService;
    }

    public async Task<IReadOnlyCollection<InstagramClientPost>> FetchPosts(string username, DateTime? afterUtc)
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

            var post = ParsePost(node);
            if (post == null || post.DateUtc <= afterUtc)
            {
                continue;
            }

            var image = await ParseImage(node);
            if (image == null)
            {
                continue;
            }

            posts.Add(new InstagramClientPost
            {
                Caption = post.Caption,
                Date = post.DateUtc,
                ExternalId = post.ExternalId,
                ImageData = image.ImageData,
                MimeType = image.MimeType,
                Url = post.Url
            });
        }

        return posts;
    }

    private static InstagramPostResponse? ParsePost(JsonNode node)
    {
        try
        {
            var unixTimestamp = node["taken_at_timestamp"]?.GetValue<int>() ?? 0;

            var date = DateUtils.FromUnixEpochTimestamp(unixTimestamp);
            var shortcode = node["shortcode"]?.ToString() ?? string.Empty;
            var caption = (node["edge_media_to_caption"]?["edges"] as JsonArray)?.FirstOrDefault()?["node"]?["text"]?.ToString() ?? string.Empty;

            var url = UrlBuilder
                .Base("https://www.instagram.com")
                .Path($"/p/{shortcode}")
                .Query("img_index", "1")
                .Build();

            return new InstagramPostResponse
            {
                Caption = caption,
                DateUtc = date,
                ExternalId = shortcode,
                Url = url
            };
        }
        catch
        {
            return null;
        }
    }

    private async Task<string?> FetchFeedJson(string username)
    {
        var url = UrlBuilder
            .Base("https://www.instagram.com")
            .Path("/api/v1/users/web_profile_info")
            .Query("username", username)
            .Build();

        var response = await _httpClient.GetAsync(url);
        var json = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            await _loggingService.Error(new Exception($"Error fetching from Instagram: {json}"), new Dictionary<string, string>());
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

    private async Task<InstagramImageResponse?> ParseImage(JsonNode node)
    {
        try
        {
            string? thumbnailUrl = node["thumbnail_src"]?.ToString();
            if (string.IsNullOrEmpty(thumbnailUrl))
            {
                return null;
            }

            var response = await _httpClient.GetAsync(thumbnailUrl);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var imageData = await response.Content.ReadAsByteArrayAsync();
            var mimeType = response.Content.Headers.ContentType?.MediaType;

            return new InstagramImageResponse
            {
                ImageData = imageData,
                MimeType = mimeType
            };
        }
        catch
        {
            return null;
        }
    }
}