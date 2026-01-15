using ODK.Core.Extensions;
using ODK.Core.Features;
using ODK.Core.SocialMedia;
using ODK.Data.Core;
using ODK.Services.Authorization;
using ODK.Services.Caching;
using ODK.Services.Imaging;
using ODK.Services.Logging;
using ODK.Services.SocialMedia.Models;
using ODK.Services.Tasks;

namespace ODK.Services.SocialMedia;

public class SocialMediaService : ISocialMediaService
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IBackgroundTaskService _backgroundTaskService;
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
        IInstagramClient instagramClient,
        IBackgroundTaskService backgroundTaskService)
    {
        _authorizationService = authorizationService;
        _backgroundTaskService = backgroundTaskService;
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
        var chapterIds = chapters
            .Select(x => x.Id)
            .ToQueue();

        ScheduleNextScrape(chapterIds, delay: false);
    }

    public Task<ServiceResult> ScrapeLatestInstagramPosts(Guid chapterId)
    {
        ScheduleNextScrape(new Queue<Guid>([chapterId]), delay: false);
        return Task.FromResult(ServiceResult.Successful("Scrape enqueued"));
    }

    // Public for Hangfire
    public async Task ScrapeLatestInstagramPosts(Queue<Guid> chapterIds)
    {
        /*This function scrapes the latest Instagram posts for each chapter, one at a time.
         *If no call was made to Instagram the next chapter is immediately enqueued,
         *otherwise the next chapter is scheduled with a delay to avoid rate limiting.*/

        if (chapterIds.Count == 0)
        {
            return;
        }

        var chapterId = chapterIds.Dequeue();

        var delayNext = false;

        var (ownerSubscription, links, lastPost) = await _unitOfWork.RunAsync(
            x => x.MemberSiteSubscriptionRepository.GetByChapterId(chapterId),
            x => x.ChapterLinksRepository.GetByChapterId(chapterId),
            x => x.InstagramPostRepository.GetLastPost(chapterId));

        if (string.IsNullOrEmpty(links?.InstagramName))
        {
            ScheduleNextScrape(chapterIds, delay: delayNext);
            return;
        }

        var authorized = _authorizationService.ChapterHasAccess(ownerSubscription, SiteFeatureType.InstagramFeed);
        if (!authorized)
        {
            ScheduleNextScrape(chapterIds, delay: delayNext);
            return;
        }

        await _loggingService.Info($"Fetching Instagram posts for group '{chapterId}' account '{links.InstagramName}'");

        var afterUtc = lastPost?.Date;

        IReadOnlyCollection<InstagramClientPost> posts;

        delayNext = true;

        try
        {
            posts = await _instagramClient.FetchPosts(links.InstagramName, afterUtc);
        }
        catch (Exception ex)
        {
            await _loggingService.Error(
                $"Error fetching Instagram posts for group '{chapterId}' account '{links.InstagramName}'", ex);
            ScheduleNextScrape(chapterIds, delay: delayNext);
            return;
        }

        if (posts.Count == 0)
        {
            await _loggingService.Info(
                $"Found no new Instagram posts for group '{chapterId}' account '{links.InstagramName}'");
            ScheduleNextScrape(chapterIds, delay: delayNext);
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

        ScheduleNextScrape(chapterIds, delay: delayNext);
    }

    private void ScheduleNextScrape(Queue<Guid> chapterIds, bool delay)
    {
        if (chapterIds.Count == 0)
        {
            return;
        }

        if (delay)
        {
            _backgroundTaskService.Schedule(
                () => ScrapeLatestInstagramPosts(chapterIds),
                DateTime.UtcNow.AddSeconds(_settings.InstagramFetchWaitSeconds));
        }
        else
        {
            _backgroundTaskService.Enqueue(() => ScrapeLatestInstagramPosts(chapterIds));
        }
    }
}