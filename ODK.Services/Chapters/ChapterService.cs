using ODK.Core.Chapters;
using ODK.Core.Platforms;
using ODK.Data.Core;
using ODK.Services.Chapters.ViewModels;
using ODK.Services.Members;
using ODK.Services.Members.ViewModels;

namespace ODK.Services.Chapters;

public class ChapterService : IChapterService
{
    private readonly ISubscriptionsPageViewModelFactory _subscriptionsPageViewModelFactory;
    private readonly IUnitOfWork _unitOfWork;

    public ChapterService(
        IUnitOfWork unitOfWork,
        ISubscriptionsPageViewModelFactory subscriptionsPageViewModelFactory)
    {
        _subscriptionsPageViewModelFactory = subscriptionsPageViewModelFactory;
        _unitOfWork = unitOfWork;
    }

    public Task<IReadOnlyCollection<Chapter>> GetApprovedChapters(PlatformType platform)
        => _unitOfWork.ChapterRepository.GetApproved(platform).Run();

    public Task<Chapter> GetByEventId(IServiceRequest request, Guid eventId)
        => _unitOfWork.ChapterRepository.GetByEventId(request.Platform, eventId).Run();

    public Task<ChapterHeaderImage?> GetChapterHeaderImage(Guid chapterId)
        => _unitOfWork.ChapterHeaderImageRepository.GetByChapterId(chapterId).Run();

    public Task<ChapterImage?> GetChapterImage(Guid chapterId)
        => _unitOfWork.ChapterImageRepository.GetByChapterId(chapterId).Run();

    public async Task<ChapterLayoutViewModel> GetChapterLayoutViewModel(Guid chapterId)
    {
        var (headerImageVersion, links, pages) = await _unitOfWork.Run(
            x => x.ChapterHeaderImageRepository.GetVersionDtoByChapterId(chapterId),
            x => x.ChapterLinksRepository.GetByChapterId(chapterId),
            x => x.ChapterPageRepository.GetByChapterId(chapterId));

        return new ChapterLayoutViewModel
        {
            HeaderImageVersion = headerImageVersion?.Version,
            Links = links,
            Pages = pages
        };
    }

    public async Task<SubscriptionsPageViewModel> GetChapterMemberSubscriptionsViewModel(
        IMemberChapterServiceRequest request)
    {
        var (environment, chapter, currentMember) =
            (request.Environment, request.Chapter, request.CurrentMember);

        var (
            memberSubscription,
            chapterSubscriptions,
            memberSubscriptionRecord,
            membershipSettings
        ) = await _unitOfWork.Run(
            x => x.MemberSubscriptionRecordRepository
                .Query()
                .Current()
                .ForMember(currentMember.Id)
                .ForChapter(chapter.Id)
                .ToChapterSubscription()
                .GetSingleOrDefault(),
            x => x.ChapterSubscriptionRepository.GetByChapterId(
                chapter.Id, environment, includeDisabled: true),
            x => x.MemberSubscriptionRecordRepository
                .Query()
                .ForMember(currentMember.Id)
                .ForChapter(chapter.Id)
                .OrderByDescending(x => x.PurchasedUtc)
                .GetSingleOrDefault(),
            x => x.ChapterMembershipSettingsRepository.GetByChapterId(chapter.Id));

        return await _subscriptionsPageViewModelFactory.Create(
            request,
            memberSubscription,
            chapterSubscriptions,
            memberSubscriptionRecord,
            membershipSettings);
    }

    public async Task<IReadOnlyCollection<Chapter>> GetChaptersByOwnerId(
        IServiceRequest request, Guid ownerId)
    {
        var platform = request.Platform;
        return await _unitOfWork.ChapterRepository.GetByOwnerId(platform, ownerId).Run();
    }

    public async Task<Chapter?> GetDefaultChapter(IMemberServiceRequest request)
    {
        var chapters = await GetMemberChapters(request);
        return chapters.FirstOrDefault();
    }

    public async Task<OdkHomeHeaderViewModel> GetOdkHomeHeaderViewModel()
    {
        var (chapters, countries) = await _unitOfWork.Run(
            x => x.ChapterRepository.GetAll(PlatformType.DrunkenKnitwits, includeUnpublished: false),
            x => x.CountryRepository.GetAll());

        var countryIds = chapters
            .Select(x => x.CountryId)
            .ToHashSet();

        countries = countries
            .Where(x => countryIds.Contains(x.Id))
            .ToArray();

        return new OdkHomeHeaderViewModel
        {
            Chapters = chapters,
            Countries = countries
        };
    }

    public async Task<Chapter?> GetSoleChapter(IMemberServiceRequest request)
    {
        var chapters = await GetMemberChapters(request);
        if (chapters.Count != 1)
        {
            return null;
        }

        // Drunken Knitwits chapters are read regardless of publication state, so an unpublished sole
        // chapter has to be filtered out here rather than relied on from the query.
        var chapter = chapters.First();
        return chapter.IsPublished() ? chapter : null;
    }

    private async Task<IReadOnlyCollection<Chapter>> GetMemberChapters(IMemberServiceRequest request)
    {
        var (platform, currentMember) = (request.Platform, request.CurrentMember);

        var chapters = await _unitOfWork.ChapterRepository
            .GetByMemberId(platform, currentMember.Id)
            .Run();

        var chapterDates = currentMember
            .Chapters
            .ToDictionary(x => x.ChapterId, x => x.CreatedUtc);

        return chapters
            .Where(x => chapterDates.ContainsKey(x.Id))
            .OrderBy(x => chapterDates[x.Id])
            .ToArray();
    }
}