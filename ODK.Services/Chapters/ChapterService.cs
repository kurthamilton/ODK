using ODK.Core;
using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Core.Payments;
using ODK.Core.Platforms;
using ODK.Data.Core;
using ODK.Services.Caching;
using ODK.Services.Chapters.ViewModels;
using ODK.Services.Members.ViewModels;
using ODK.Services.Payments;

namespace ODK.Services.Chapters;

public class ChapterService : IChapterService
{
    private readonly ICacheService _cacheService;
    private readonly IPaymentProviderFactory _paymentProviderFactory;
    private readonly IUnitOfWork _unitOfWork;

    public ChapterService(
        IUnitOfWork unitOfWork,
        ICacheService cacheService,
        IPaymentProviderFactory paymentProviderFactory)
    {
        _cacheService = cacheService;
        _paymentProviderFactory = paymentProviderFactory;
        _unitOfWork = unitOfWork;
    }

    public Task<Chapter> GetByEventId(IServiceRequest request, Guid eventId)
        => _unitOfWork.ChapterRepository.GetByEventId(request.Platform, eventId).Run();

    public async Task<VersionedServiceResult<ChapterImage>> GetChapterImage(long? currentVersion, Guid chapterId)
    {
        var result = await _cacheService.GetOrSetVersionedItem(
            () => _unitOfWork.ChapterImageRepository.GetByChapterId(chapterId).Run(),
            chapterId,
            currentVersion);

        if (currentVersion == result.Version)
        {
            return result;
        }

        var image = result.Value;
        return image != null
            ? new VersionedServiceResult<ChapterImage>(BitConverter.ToInt64(image.Version), image)
            : new VersionedServiceResult<ChapterImage>(0, null);
    }

    public async Task<ChapterLayoutViewModel> GetChapterLayoutViewModel(Guid chapterId)
    {
        var (links, pages) = await _unitOfWork.RunAsync(
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
        var (chapter, platform, currentMember) =
            (request.Chapter, request.Platform, request.CurrentMember);

        var (
            memberSubscription,
            chapterSubscriptions,
            sitePaymentSettings,
            memberSubscriptionRecord,
            membershipSettings
        ) = await _unitOfWork.RunAsync(
            x => x.MemberSubscriptionRepository.GetByMemberId(currentMember.Id, chapter.Id),
            x => x.ChapterSubscriptionRepository.GetByChapterId(chapter.Id, includeDisabled: true),
            x => x.SitePaymentSettingsRepository.GetAll(),
            x => x.MemberSubscriptionRecordRepository.GetLatest(currentMember.Id, chapter.Id),
            x => x.ChapterMembershipSettingsRepository.GetByChapterId(chapter.Id));

        OdkAssertions.MemberOf(currentMember, chapter.Id);

        var currentSubscription = chapterSubscriptions
            .FirstOrDefault(x => x.Id == memberSubscriptionRecord?.ChapterSubscriptionId);

        chapterSubscriptions = chapterSubscriptions
            .Where(x => x.IsVisibleToMembers(sitePaymentSettings))
            .ToArray();

        var externalSubscription = await GetExternalSubscription(
            sitePaymentSettings,
            memberSubscriptionRecord,
            chapterSubscriptions);

        return new SubscriptionsPageViewModel
        {
            Chapter = chapter,
            ChapterSubscriptions = chapterSubscriptions,
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

    public async Task<ChaptersHomePageViewModel> GetChaptersHomePageViewModel(PlatformType platform)
    {
        var (chapters, countries) = await _unitOfWork.RunAsync(
            x => x.ChapterRepository.GetAll(platform, includeUnpublished: false),
            x => x.CountryRepository.GetAll());

        if (platform != PlatformType.Default)
        {
            var countryIds = chapters
                .Select(x => x.CountryId)
                .ToHashSet();

            countries = countries
                .Where(x => countryIds.Contains(x.Id))
                .ToArray();
        }

        return new ChaptersHomePageViewModel
        {
            Chapters = chapters,
            Countries = countries
        };
    }

    public async Task<Chapter?> GetDefaultChapter(IMemberServiceRequest request)
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
            .FirstOrDefault();
    }

    private async Task<ExternalSubscription?> GetExternalSubscription(
        IReadOnlyCollection<SitePaymentSettings> sitePaymentSettings,
        MemberSubscriptionRecord? memberSubscriptionRecord,
        IReadOnlyCollection<ChapterSubscription> chapterSubscriptions)
    {
        if (memberSubscriptionRecord?.ExternalId == null ||
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

        var paymentProvider = _paymentProviderFactory.GetSitePaymentProvider(
            sitePaymentSettings,
            chapterSubscription.SitePaymentSettingId);

        return await paymentProvider.GetSubscription(memberSubscriptionRecord.ExternalId);
    }
}