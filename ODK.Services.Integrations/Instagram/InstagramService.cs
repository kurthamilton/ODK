using System.Text.Json.Nodes;
using ODK.Core.Features;
using ODK.Core.SocialMedia;
using ODK.Core.Utils;
using ODK.Core.Web;
using ODK.Data.Core;
using ODK.Services.Authorization;
using ODK.Services.Caching;
using ODK.Services.Imaging;
using ODK.Services.Logging;
using ODK.Services.SocialMedia;

namespace ODK.Services.Integrations.Instagram;

public class InstagramService : IInstagramService
{
    private readonly IAuthorizationService _authorizationService;
    private readonly ICacheService _cacheService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IImageService _imageService;
    private readonly ILoggingService _loggingService;
    private readonly IUnitOfWork _unitOfWork;

    public InstagramService(
        IUnitOfWork unitOfWork, 
        ILoggingService loggingService, 
        ICacheService cacheService,
        IImageService imageService,
        IAuthorizationService authorizationService,
        IHttpClientFactory httpClientFactory)
    {
        _authorizationService = authorizationService;
        _cacheService = cacheService;
        _httpClientFactory = httpClientFactory;
        _imageService = imageService;
        _loggingService = loggingService;
        _unitOfWork = unitOfWork;
    }

    public async Task<VersionedServiceResult<InstagramImage>> GetInstagramImage(long? currentVersion, Guid instagramPostId)
    {
        var result = await _cacheService.GetOrSetVersionedItem(
            async () => await _unitOfWork.InstagramImageRepository.GetByPostId(instagramPostId).Run(),
            instagramPostId,
            currentVersion);

        if (currentVersion == result.Version)
        {
            return result;
        }

        var image = result.Value;
        return image != null
            ? new VersionedServiceResult<InstagramImage>(BitConverter.ToInt64(image.Version), image)
            : new VersionedServiceResult<InstagramImage>(0, null);
    }

    public async Task ScrapeLatestInstagramPosts()
    {
        var chapters = await _unitOfWork.ChapterRepository.GetAll().Run();
        foreach (var chapter in chapters)
        {
            var authorized = await _authorizationService.ChapterHasAccess(chapter, SiteFeatureType.InstagramFeed);
            if (!authorized)
            {
                continue;
            }

            try
            {
                await ScrapeLatestInstagramPosts(chapter.Id);
            }            
            catch
            {
                // do nothing
            }
        }
    }

    public async Task ScrapeLatestInstagramPosts(Guid chapterId)
    {
        var (settings, links, lastPost) = await _unitOfWork.RunAsync(
            x => x.SiteSettingsRepository.Get(),
            x => x.ChapterLinksRepository.GetByChapterId(chapterId),
            x => x.InstagramPostRepository.GetLastPost(chapterId));

        if (string.IsNullOrEmpty(links?.InstagramName))
        {
            return;
        }

        var after = lastPost?.Date;

        using var httpClient = CreateHttpClient(settings.InstagramScraperUserAgent);

        await DownloadNewImages(
            httpClient, 
            chapterId, 
            links.InstagramName, 
            after);
    }

    private HttpClient CreateHttpClient(string userAgent)
    {
        var httpClient = _httpClientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Add("User-Agent", userAgent);
        return httpClient;
    }

    private async Task DownloadNewImages(
        HttpClient httpClient, Guid chapterId, string username, DateTime? after)
    {
        var feedJson = await FetchFeedJson(httpClient, username);
        if (string.IsNullOrEmpty(feedJson))
        {
            return;
        }

        var edges = ParseFeedEdges(feedJson);
        if (edges == null)
        {
            return;
        }

        foreach (var edge in edges)
        {
            var node = edge?["node"];

            var post = ParsePost(chapterId, node);
            if (post == null || post.Date <= after)
            {
                continue;
            }
            
            var image = await ParseImage(httpClient, post, node);
            if (image == null)
            {
                continue;
            }

            _unitOfWork.InstagramPostRepository.Add(post);
            _unitOfWork.InstagramImageRepository.Add(image);
        }

        await _unitOfWork.SaveChangesAsync();
    }

    private async Task<string?> FetchFeedJson(HttpClient httpClient, string username)
    {
        var url = UrlBuilder
            .Base("https://www.instagram.com")
            .Path("/api/v1/users/web_profile_info")
            .Query("username", username)
            .Build();

        var response = await httpClient.GetAsync(url);
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

    private async Task<InstagramImage?> ParseImage(HttpClient httpClient, InstagramPost post, JsonNode? node)
    {
        if (node == null)
        {
            return null;
        }

        try
        {
            string? thumbnailUrl = node["thumbnail_src"]?.ToString();
            if (string.IsNullOrEmpty(thumbnailUrl))
            {
                return null;
            }
            
            var response = await httpClient.GetAsync(thumbnailUrl);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var imageData = await response.Content.ReadAsByteArrayAsync();
            var mimeType = response.Content.Headers.ContentType?.MediaType;

            imageData = _imageService.Reduce(imageData, 250, 250);

            return new InstagramImage
            {
                ImageData = imageData,
                InstagramPostId = post.Id,
                MimeType = mimeType ?? ""
            };
        }
        catch
        {
            return null;
        }
    }

    private static InstagramPost? ParsePost(Guid chapterId, JsonNode? node)
    {
        if (node == null)
        {
            return null;
        }

        try
        {
            var unixTimestamp = node["taken_at_timestamp"]?.GetValue<int>() ?? 0;
            
            var date = DateUtils.FromUnixEpochTimestamp(unixTimestamp);
            var shortcode = node["shortcode"]?.ToString() ?? "";
            var caption = (node["edge_media_to_caption"]?["edges"] as JsonArray)?.FirstOrDefault()?["node"]?["text"]?.ToString() ?? "";
            
            var url = UrlBuilder
                .Base("https://www.instagram.com")
                .Path($"/p/{shortcode}")
                .Query("img_index", "1")
                .Build();

            return new InstagramPost
            {
                Caption = caption,
                ChapterId = chapterId,
                Date = date,
                ExternalId = shortcode,
                Id = Guid.NewGuid(),
                Url = url
            };
        }
        catch
        {
            return null;
        }
    }
}
