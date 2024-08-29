using Newtonsoft.Json.Linq;
using ODK.Core.Chapters;
using ODK.Core.Features;
using ODK.Core.SocialMedia;
using ODK.Data.Core;
using ODK.Services.Authorization;
using ODK.Services.Caching;
using ODK.Services.Imaging;
using ODK.Services.Logging;

namespace ODK.Services.SocialMedia;

public class InstagramService : IInstagramService
{
    private readonly IAuthorizationService _authorizationService;
    private readonly ICacheService _cacheService;
    private readonly IImageService _imageService;
    private readonly ILoggingService _loggingService;
    private readonly IUnitOfWork _unitOfWork;

    public InstagramService(
        IUnitOfWork unitOfWork, 
        ILoggingService loggingService, 
        ICacheService cacheService,
        IImageService imageService,
        IAuthorizationService authorizationService)
    {
        _authorizationService = authorizationService;
        _cacheService = cacheService;
        _imageService = imageService;
        _loggingService = loggingService;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyCollection<SocialMediaImage>> FetchInstagramImages(Guid chapterId)
    {
        await ScrapeLatestInstagramPosts(chapterId);

        var posts = await _unitOfWork.InstagramPostRepository.GetByChapterId(chapterId, 8).Run();

        return posts
            .Select(x => new SocialMediaImage
            {
                Caption = x.Caption,
                Url = x.Url
            })
            .ToArray();
    }

    public async Task<VersionedServiceResult<InstagramImage>> GetInstagramImage(long? currentVersion, Guid instagramPostId)
    {
        var result = await _cacheService.GetOrSetVersionedItem(
            async () =>
            {
                var result = await _unitOfWork.InstagramImageRepository.GetByPostId(instagramPostId).Run();
                return result;
            },
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

    public async Task<InstagramPostsDto> GetInstagramPosts(Guid chapterId, int pageSize)
    {
        var (settings, links, posts) = await _unitOfWork.RunAsync(
            x => x.SiteSettingsRepository.Get(),
            x => x.ChapterLinksRepository.GetByChapterId(chapterId),
            x => x.InstagramPostRepository.GetByChapterId(chapterId, pageSize));

        if (string.IsNullOrEmpty(links?.InstagramName))
        {
            return new InstagramPostsDto
            {
                Links = new ChapterLinks { ChapterId = chapterId },
                Posts = []
            };
        }

        return new InstagramPostsDto
        {
            Links = links,
            Posts = posts
        };
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

        await DownloadNewImages(chapterId, settings.InstagramScraperUserAgent,
            links.InstagramName, after);
    }

    private async Task DownloadNewImages(Guid chapterId, string userAgent, string username, DateTime? after)
    {
        string json;

        using (var httpClient = new HttpClient())
        {
            httpClient.DefaultRequestHeaders.Add("User-Agent", userAgent);
            string url = $"https://www.instagram.com/api/v1/users/web_profile_info/?username={username}";
            HttpResponseMessage response = await httpClient.GetAsync(url);
            json = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                await _loggingService.LogError(new Exception($"Error fetching from Instagram: {json}"), new Dictionary<string, string>());
                return;
            }
        }

        JArray? edges;
        try
        {
            JObject data = JObject.Parse(json);
            edges = data["data"]?["user"]?["edge_owner_to_timeline_media"]?["edges"] as JArray;
            if (edges == null)
            {
                return;
            }
        }
        catch
        {
            return;
        }

        foreach (JToken edge in edges)
        {
            JToken? node = edge["node"];

            var post = ParsePost(chapterId, node);
            if (post == null || post.Date <= after)
            {
                continue;
            }
            
            var image = await ParseImage(post, userAgent, node);
            if (image == null)
            {
                continue;
            }

            _unitOfWork.InstagramPostRepository.Add(post);
            _unitOfWork.InstagramImageRepository.Add(image);
        }

        await _unitOfWork.SaveChangesAsync();
    }

    private async Task<InstagramImage?> ParseImage(InstagramPost post, string userAgent, JToken? node)
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

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", userAgent);

            var response = await client.GetAsync(thumbnailUrl);
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

    private static InstagramPost? ParsePost(Guid chapterId, JToken? node)
    {
        if (node == null)
        {
            return null;
        }

        try
        {
            var timestamp = node["taken_at_timestamp"]?.ToObject<int>() ?? 0;
            var date = new DateTime(1970, 1, 1).AddSeconds(timestamp);
            var shortcode = node["shortcode"]?.ToString() ?? "";
            var caption = node["edge_media_to_caption"]?["edges"]?.FirstOrDefault()?["node"]?["text"]?.ToString() ?? "";
            var url = $"https://www.instagram.com/p/{shortcode}/?img_index=1";

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
