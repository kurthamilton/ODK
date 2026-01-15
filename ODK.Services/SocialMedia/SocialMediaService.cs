using ODK.Core.Features;
using ODK.Core.SocialMedia;
using ODK.Data.Core;
using ODK.Services.Authorization;
using ODK.Services.Caching;
using ODK.Services.Imaging;
using ODK.Services.Logging;

namespace ODK.Services.SocialMedia;

public class SocialMediaService : ISocialMediaService
{
    private readonly IAuthorizationService _authorizationService;
    private readonly ICacheService _cacheService;
    private readonly IImageService _imageService;
    private readonly IInstagramClient _instagramClient;
    private readonly ILoggingService _loggingService;
    private readonly SocialMediaServiceSettings _settings;
    private readonly IUnitOfWork _unitOfWork;

    public SocialMediaService(
        SocialMediaServiceSettings settings,
        IUnitOfWork unitOfWork,
        ILoggingService loggingService,
        ICacheService cacheService,
        IImageService imageService,
        IAuthorizationService authorizationService,
        IInstagramClient instagramClient)
    {
        _authorizationService = authorizationService;
        _cacheService = cacheService;
        _imageService = imageService;
        _instagramClient = instagramClient;
        _loggingService = loggingService;
        _settings = settings;
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

    public string GetWhatsAppLink(string groupId) => _settings.WhatsAppUrlFormat.Replace("{groupid}", groupId);

    public async Task ScrapeLatestInstagramPosts()
    {
        await _loggingService.Info("Scraping latest Instagram posts for all groups");

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
        var (links, lastPost) = await _unitOfWork.RunAsync(
            x => x.ChapterLinksRepository.GetByChapterId(chapterId),
            x => x.InstagramPostRepository.GetLastPost(chapterId));

        if (string.IsNullOrEmpty(links?.InstagramName))
        {
            return;
        }

        await _loggingService.Info($"Scraping Instagram posts for group '{chapterId}' account '{links.InstagramName}'");

        var afterUtc = lastPost?.Date;

        var posts = await _instagramClient.FetchPosts(links.InstagramName, afterUtc);
        if (posts.Count == 0)
        {
            return;
        }

        foreach (var post in posts)
        {
            var id = Guid.NewGuid();

            _unitOfWork.InstagramPostRepository.Add(new InstagramPost
            {
                Caption = post.Caption,
                ChapterId = chapterId,
                Date = post.Date,
                ExternalId = post.ExternalId,
                Id = id,
                Url = post.Url
            });

            _unitOfWork.InstagramImageRepository.Add(new InstagramImage
            {
                ImageData = _imageService.Reduce(post.ImageData, 250, 250),
                InstagramPostId = id,
                MimeType = post.MimeType ?? string.Empty
            });
        }

        try
        {
            await _unitOfWork.SaveChangesAsync();

            await _loggingService.Info(
                $"Saved {posts.Count} new Instagram posts for group '{chapterId}' account '{links.InstagramName}'");
        }
        catch (Exception ex)
        {
            await _loggingService.Error(
                $"Error saving {posts.Count} new Instagram posts for group '{chapterId}' account '{links.InstagramName}'", ex);
        }
    }
}