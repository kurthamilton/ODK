using ODK.Core.Extensions;
using ODK.Core.Features;
using ODK.Core.Platforms;
using ODK.Core.SocialMedia;
using ODK.Core.Utils;
using ODK.Data.Core;
using ODK.Services.Authorization;
using ODK.Services.Logging;
using ODK.Services.Tasks;

namespace ODK.Services.SocialMedia;

public class SocialMediaService : ISocialMediaService
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IBackgroundTaskService _backgroundTaskService;
    private readonly IInstagramClient _instagramClient;
    private readonly ILoggingService _loggingService;
    private readonly SocialMediaServiceSettings _settings;
    private readonly IUnitOfWork _unitOfWork;

    public SocialMediaService(
        SocialMediaServiceSettings settings,
        IUnitOfWork unitOfWork,
        ILoggingService loggingService,
        IAuthorizationService authorizationService,
        IInstagramClient instagramClient,
        IBackgroundTaskService backgroundTaskService)
    {
        _authorizationService = authorizationService;
        _backgroundTaskService = backgroundTaskService;
        _instagramClient = instagramClient;
        _loggingService = loggingService;
        _settings = settings;
        _unitOfWork = unitOfWork;
    }

    public string GetInstagramChannelUrl(string username)
        => _settings.InstagramChannelUrlFormat.Replace("{username}", username);

    public string GetInstagramHashtagUrl(string hashtag)
    {
        var tag = StringUtils.RemoveLeading(hashtag, "#");
        return _settings.InstagramTagUrlFormat.Replace("{tag}", tag);
    }

    public async Task<InstagramImage> GetInstagramImage(Guid id)
    {
        return await _unitOfWork.InstagramImageRepository.GetById(id).Run();
    }

    public string GetInstagramPostUrl(string externalId)
        => _settings.InstagramPostUrlFormat.Replace("{id}", externalId);

    public string GetWhatsAppLink(string groupId) => _settings.WhatsAppUrlFormat.Replace("{groupid}", groupId);

    public async Task ScrapeLatestInstagramPosts()
    {
        await _loggingService.Info("Scraping latest Instagram posts for all groups");

        var chapters = await _unitOfWork.ChapterRepository
            .GetAll(PlatformType.Default, includeUnpublished: false)
            .Run();

        var chapterIds = chapters
            .Select(x => x.Id)
            .ToQueue();

        await ScheduleNextScrape(chapterIds, runNow: true);
    }

    public async Task<ServiceResult> ScrapeLatestInstagramPosts(Guid chapterId)
    {
        await ScheduleNextScrape(new Queue<Guid>([chapterId]), runNow: true);
        return ServiceResult.Successful("Scrape enqueued");
    }

    // Public for Hangfire
    public async Task ScrapeLatestInstagramPosts(Queue<Guid> chapterIds, int delaySeconds)
    {
        /*This function scrapes the latest Instagram posts for each chapter, one at a time.
         *If no call was made to Instagram the next chapter is immediately enqueued,
         *otherwise the next chapter is scheduled with a delay to avoid rate limiting.*/

        if (chapterIds.Count == 0)
        {
            return;
        }

        var chapterId = chapterIds.Dequeue();

        var (ownerSubscription, links) = await _unitOfWork.RunAsync(
            x => x.MemberSiteSubscriptionRepository.GetByChapterId(chapterId),
            x => x.ChapterLinksRepository.GetByChapterId(chapterId));

        if (string.IsNullOrEmpty(links?.InstagramName))
        {
            await ScheduleNextScrape(chapterIds, runNow: true);
            return;
        }

        var authorized = _authorizationService.ChapterHasAccess(ownerSubscription, SiteFeatureType.InstagramFeed);
        if (!authorized)
        {
            await ScheduleNextScrape(chapterIds, runNow: true);
            return;
        }

        await _loggingService.Info(
            $"Fetching Instagram posts for group '{chapterId}' account '{links.InstagramName}'");

        var result = await _instagramClient.FetchLatestPosts(links.InstagramName);

        _unitOfWork.InstagramFetchLogEntryRepository.Add(new InstagramFetchLogEntry
        {
            CreatedUtc = DateTime.UtcNow,
            DelaySeconds = delaySeconds,
            Response = result.Response,
            Success = result.Success,
            Username = links.InstagramName
        });
        await _unitOfWork.SaveChangesAsync();

        var posts = result.Posts;
        if (posts == null || posts.Count == 0)
        {
            await _loggingService.Info(
                $"Error fetching Instagram posts for group '{chapterId}' account '{links.InstagramName}'");
            await ScheduleNextScrape(chapterIds, runNow: false);
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

            var postId = Guid.NewGuid();

            _unitOfWork.InstagramPostRepository.Add(new InstagramPost
            {
                Caption = post.Caption,
                ChapterId = chapterId,
                Date = post.Date,
                ExternalId = post.ExternalId,
                Id = postId
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
                    InstagramPostId = postId,
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

        await ScheduleNextScrape(chapterIds, runNow: false);
    }

    private int GetNextFetchWaitSeconds(
        InstagramFetchLogEntry? last, IReadOnlyCollection<InstagramFetchLogEntry> recentSuccessful)
    {
        var delaySeconds = _settings.InstagramFetchWaitSeconds;

        if (last == null)
        {
            // No recent log entries - start off with default
            return delaySeconds;
        }

        if (!last.Success)
        {
            // The most recent fetch failed - increase the delay it used
            return last.DelaySeconds + delaySeconds;
        }

        // The most recent fetch succeeded - use the average of recent successful fetches
        // In theory this number will converge around the sweet spot over time.
        var average = recentSuccessful
            .Select(x => 1.0 * x.DelaySeconds)
            .Average();

        return (int)Math.Ceiling(average);
    }

    private async Task ScheduleNextScrape(Queue<Guid> chapterIds, bool runNow)
    {
        if (chapterIds.Count == 0)
        {
            return;
        }

        var (recentSuccessful, last) = await _unitOfWork.RunAsync(
            x => x.InstagramFetchLogEntryRepository.GetRecentSuccessful(10),
            x => x.InstagramFetchLogEntryRepository.GetLast());

        var delaySeconds = GetNextFetchWaitSeconds(last, recentSuccessful);

        var utcNow = DateTime.UtcNow;
        var nextEarliestRunUtc = last == null || (utcNow - last.CreatedUtc).TotalSeconds > delaySeconds
            ? utcNow
            : last.CreatedUtc.AddSeconds(delaySeconds);

        if (runNow)
        {
            _backgroundTaskService.Schedule(
                () => ScrapeLatestInstagramPosts(chapterIds, delaySeconds),
                nextEarliestRunUtc,
                BackgroundTaskQueueType.Instagram);
        }
        else
        {
            var randomDelaySeconds = Random.Shared.Next(_settings.InstagramFetchWaitSeconds);

            _backgroundTaskService.Schedule(
                () => ScrapeLatestInstagramPosts(chapterIds, delaySeconds),
                nextEarliestRunUtc.AddSeconds(randomDelaySeconds),
                BackgroundTaskQueueType.Instagram);
        }
    }
}