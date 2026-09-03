using ODK.Core.Chapters;
using ODK.Data.Core;
using ODK.Data.Core.Events;
using ODK.Services.Sitemap.ViewModels;

namespace ODK.Services.Sitemap;

public class SitemapService : ISitemapService
{
    private readonly IUnitOfWork _unitOfWork;

    public SitemapService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<SitemapViewModel> GetSitemapViewModel(IServiceRequest request)
    {
        var approved = await _unitOfWork.ChapterRepository
            .GetApproved(request.Platform)
            .Run();

        // Approval is not publication on the Drunken Knitwits platform - its chapter queries do not filter
        // on it - and a chapter that is not open for registration renders a stub in place of its content.
        var chapters = approved
            .Where(x => x.IsOpenForRegistration())
            .OrderBy(x => x.GetDisplayName(request.Platform))
            .ToArray();

        if (chapters.Length == 0)
        {
            return new SitemapViewModel
            {
                Chapters = []
            };
        }

        var chapterIds = chapters
            .Select(x => x.Id)
            .ToArray();

        var (chapterPages, privacySettings, questionCounts) = await _unitOfWork.Run(
            x => x.ChapterPageRepository.GetByChapterIds(chapterIds),
            x => x.ChapterPrivacySettingsRepository.GetByChapterIds(chapterIds),
            x => x.ChapterQuestionRepository.GetCountsByChapterIds(chapterIds));

        var privacySettingsByChapterId = privacySettings
            .ToDictionary(x => x.ChapterId);

        // Events are read only for the groups that show them publicly, which is the minority: the default
        // is ActiveMembers. That is why this is a query of its own rather than part of the batch above -
        // which chapters to ask about is the answer to it.
        var publicEventChapterIds = chapters
            .Where(x => IsPublic(privacySettingsByChapterId, x.Id, ChapterFeatureType.Events))
            .Select(x => x.Id)
            .ToArray();

        IReadOnlyCollection<EventPublicationDto> events = publicEventChapterIds.Length > 0
            ? await _unitOfWork.EventRepository
                .Query()
                .ForChapters(publicEventChapterIds)
                .Published()
                .Publication()
                .GetAll()
                .Run()
            : [];

        var chapterPagesByChapterId = chapterPages
            .GroupBy(x => x.ChapterId)
            .ToDictionary(x => x.Key, x => (IReadOnlyCollection<ChapterPage>)x.ToArray());

        var eventsByChapterId = events
            .GroupBy(x => x.ChapterId)
            .ToDictionary(
                x => x.Key,
                x => (IReadOnlyCollection<EventPublicationDto>)x
                    .OrderByDescending(y => y.PublishedUtc)
                    .ThenBy(y => y.Shortcode)
                    .ToArray());

        var chapterIdsWithQuestions = questionCounts
            .Where(x => x.Count > 0)
            .Select(x => x.ChapterId)
            .ToHashSet();

        return new SitemapViewModel
        {
            Chapters = chapters
                .Select(x => new SitemapChapterViewModel
                {
                    Chapter = x,
                    ChapterPages = chapterPagesByChapterId.TryGetValue(x.Id, out var pages) ? pages : [],
                    Events = eventsByChapterId.TryGetValue(x.Id, out var chapterEvents) ? chapterEvents : [],
                    HasQuestions = chapterIdsWithQuestions.Contains(x.Id),
                    PrivacySettings = privacySettingsByChapterId.TryGetValue(x.Id, out var settings)
                        ? settings
                        : null
                })
                .ToArray()
        };
    }

    private static bool IsPublic(
        IReadOnlyDictionary<Guid, ChapterPrivacySettings> privacySettingsByChapterId,
        Guid chapterId,
        ChapterFeatureType feature)
    {
        privacySettingsByChapterId.TryGetValue(chapterId, out var settings);
        return settings.Visibility(feature) == ChapterFeatureVisibilityType.Public;
    }
}
