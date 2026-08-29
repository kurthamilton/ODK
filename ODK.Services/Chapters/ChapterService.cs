using ODK.Core;
using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Core.Payments;
using ODK.Core.Platforms;
using ODK.Data.Core;
using ODK.Services.Chapters.ViewModels;
using ODK.Services.Members.ViewModels;
using ODK.Services.Payments;

namespace ODK.Services.Chapters;

public class ChapterService : IChapterService
{
    private readonly IPaymentProviderFactory _paymentProviderFactory;
    private readonly IUnitOfWork _unitOfWork;

    public ChapterService(
        IUnitOfWork unitOfWork,
        IPaymentProviderFactory paymentProviderFactory)
    {
        _paymentProviderFactory = paymentProviderFactory;
        _unitOfWork = unitOfWork;
    }

    public Task<IReadOnlyCollection<Chapter>> GetApprovedChapters(PlatformType platform)
        => _unitOfWork.ChapterRepository.GetApproved(platform).Run();

    public Task<Chapter> GetByEventId(IServiceRequest request, Guid eventId)
        => _unitOfWork.ChapterRepository.GetByEventId(request.Platform, eventId).Run();

    public Task<ChapterImage?> GetChapterImage(Guid chapterId)
        => _unitOfWork.ChapterImageRepository.GetByChapterId(chapterId).Run();

    public async Task<ChapterLayoutViewModel> GetChapterLayoutViewModel(Guid chapterId)
    {
        var (links, pages) = await _unitOfWork.Run(
            x => x.ChapterLinksRepository.GetByChapterId(chapterId),
            x => x.ChapterPageRepository.GetByChapterId(chapterId));

        return new ChapterLayoutViewModel
        {
            Links = links,
            Pages = pages
        };
    }

    public async Task<SubscriptionsPageViewModel> GetChapterMemberSubscriptionsViewModel(
        IMemberChapterServiceRequest request)
    {
        var (environment, chapter, platform, currentMember) =
            (request.Environment, request.Chapter, request.Platform, request.CurrentMember);

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

        OdkAssertions.MemberOf(currentMember, chapter.Id);

        var currentSubscription = chapterSubscriptions
            .FirstOrDefault(x => x.Id == memberSubscriptionRecord?.ChapterSubscriptionId);

        chapterSubscriptions = chapterSubscriptions
            .Where(x => x.IsVisibleToMembers())
            .ToArray();

        var externalSubscription = await GetExternalSubscription(
            chapter,
            memberSubscriptionRecord,
            chapterSubscriptions);

        return new SubscriptionsPageViewModel
        {
            Chapter = chapter,
            ChapterSubscriptions = chapterSubscriptions,
            CurrentMember = currentMember,
            CurrentSubscription = currentSubscription,
            ExternalSubscription = externalSubscription,
            MembershipSettings = membershipSettings,
            MemberSubscription = memberSubscription
        };
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

    private async Task<ExternalSubscription?> GetExternalSubscription(
        Chapter chapter,
        MemberSubscriptionRecord? memberSubscriptionRecord,
        IReadOnlyCollection<ChapterSubscription> chapterSubscriptions)
    {
        if (string.IsNullOrEmpty(memberSubscriptionRecord?.ExternalId) ||
            memberSubscriptionRecord.ChapterSubscriptionId == null)
        {
            return null;
        }

        var chapterSubscription = chapterSubscriptions
            .FirstOrDefault(x => x.Id == memberSubscriptionRecord.ChapterSubscriptionId);

        if (chapterSubscription == null)
        {
            return null;
        }

        var paymentProvider = _paymentProviderFactory.GetPaymentProvider(
            chapterSubscription.PaymentProvider, chapter.Platform);

        return await paymentProvider.GetSubscription(memberSubscriptionRecord.ExternalId);
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