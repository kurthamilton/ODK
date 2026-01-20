using ODK.Core.Extensions;
using ODK.Core.Features;
using ODK.Core.SocialMedia;
using ODK.Core.Utils;
using ODK.Data.Core;
using ODK.Services.Authorization;
using ODK.Services.Caching;
using ODK.Services.Logging;
using ODK.Services.SocialMedia.Models;
using ODK.Services.Tasks;

namespace ODK.Services.SocialMedia;

public class SocialMediaService : ISocialMediaService
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IBackgroundTaskService _backgroundTaskService;
    private readonly ICacheService _cacheService;
    private readonly IInstagramClient _instagramClient;
    private readonly ILoggingService _loggingService;
    private readonly SocialMediaServiceSettings _settings;
    private readonly IUnitOfWork _unitOfWork;

    public SocialMediaService(
        SocialMediaServiceSettings settings,
        IUnitOfWork unitOfWork,
        ILoggingService loggingService,
        ICacheService cacheService,
        IAuthorizationService authorizationService,
        IInstagramClient instagramClient,
        IBackgroundTaskService backgroundTaskService)
    {
        _authorizationService = authorizationService;
        _backgroundTaskService = backgroundTaskService;
        _cacheService = cacheService;
        _instagramClient = instagramClient;
        _loggingService = loggingService;
        _settings = settings;
        _unitOfWork = unitOfWork;
    }

    public async Task<VersionedServiceResult<InstagramImage>> GetInstagramImage(long? currentVersion, Guid id)
    {
        var result = await _cacheService.GetOrSetVersionedItem(
            async () => await _unitOfWork.InstagramImageRepository.GetById(id).Run(),
            id,
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

    public string GetInstagramChannelUrl(string username)
        => _settings.InstagramChannelUrlFormat.Replace("{username}", username);

    public string GetInstagramHashtagUrl(string hashtag)
    {
        var tag = StringUtils.RemoveLeading(hashtag, "#");
        return _settings.InstagramTagUrlFormat.Replace("{tag}", tag);
    }

    public string GetInstagramPostUrl(string externalId)
        => _settings.InstagramPostUrlFormat.Replace("{id}", externalId);

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

        var (ownerSubscription, links) = await _unitOfWork.RunAsync(
            x => x.MemberSiteSubscriptionRepository.GetByChapterId(chapterId),
            x => x.ChapterLinksRepository.GetByChapterId(chapterId));

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

        await _loggingService.Info(
            $"Fetching Instagram posts for group '{chapterId}' account '{links.InstagramName}'");

        IReadOnlyCollection<InstagramClientPost> posts;

        delayNext = true;

        try
        {
            posts = await _instagramClient.FetchLatestPosts(links.InstagramName);
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

        var externalIds = posts
            .Select(x => x.ExternalId)
            .ToArray();

        var existingPosts = await _unitOfWork.InstagramPostRepository.GetDtosByExternalIds(externalIds).Run();
        var existingPostDictionary = existingPosts
            .ToDictionary(x => x.Post.ExternalId, StringComparer.OrdinalIgnoreCase);

        foreach (var post in posts)
        {
            if (existingPostDictionary.TryGetValue(post.ExternalId, out var existingPost))
            {
                var existingImageIds = existingPost.Images
                    .Select(x => x.ExternalId)
                    .ToArray();

                var imageIds = post.Images
                    .Select(x => x.ExternalId)
                    .ToArray();

                if (existingImageIds.EquivalentTo(imageIds, StringComparer.OrdinalIgnoreCase))
                {
                    // no need to re-download if the post we already have has the same image ids
                    continue;
                }

                _unitOfWork.InstagramPostRepository.Delete(existingPost.Post);
            }

            var fetchImageTasks = post.Images
                .Select(_instagramClient.FetchImage)
                .ToArray();

            await Task.WhenAll(fetchImageTasks);

            var id = Guid.NewGuid();

            _unitOfWork.InstagramPostRepository.Add(new InstagramPost
            {
                Caption = post.Caption,
                ChapterId = chapterId,
                Date = post.Date,
                ExternalId = post.ExternalId,
                Id = id
            });

            for (var i = 0; i < post.Images.Count; i++)
            {
                var metadata = post.Images.ElementAt(i);
                var image = fetchImageTasks.ElementAt(i).Result;

                _unitOfWork.InstagramImageRepository.Add(new InstagramImage
                {
                    Alt = metadata.Alt,
                    DisplayOrder = i + 1,
                    ExternalId = metadata.ExternalId,
                    Height = metadata.Height,
                    ImageData = image.ImageData,
                    InstagramPostId = id,
                    IsVideo = metadata.IsVideo,
                    MimeType = image.MimeType ?? string.Empty,
                    Width = metadata.Width
                });
            }
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