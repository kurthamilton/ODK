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

    public async Task<ChapterLinks?> GetChapterLinks(Guid chapterId)
    {
        return await _unitOfWork.ChapterLinksRepository.GetByChapterId(chapterId).Run();
    }

    public async Task<SubscriptionsPageViewModel> GetChapterMemberSubscriptionsViewModel(
        MemberChapterServiceRequest request)
    {
        var (chapterId, platform, currentMemberId) =
            (request.ChapterId, request.Platform, request.CurrentMemberId);

        var (
            chapter,
            currentMember,
            memberSubscription,
            chapterSubscriptions,
            chapterPaymentSettings,
            sitePaymentSettings,
            memberSubscriptionRecord,
            membershipSettings
        ) = await _unitOfWork.RunAsync(
            x => x.ChapterRepository.GetById(chapterId),
            x => x.MemberRepository.GetById(currentMemberId),
            x => x.MemberSubscriptionRepository.GetByMemberId(currentMemberId, chapterId),
            x => x.ChapterSubscriptionRepository.GetDtosByChapterId(chapterId, includeDisabled: true),
            x => x.ChapterPaymentSettingsRepository.GetByChapterId(chapterId),
            x => x.SitePaymentSettingsRepository.GetAll(),
            x => x.MemberSubscriptionRecordRepository.GetLatest(currentMemberId, chapterId),
            x => x.ChapterMembershipSettingsRepository.GetByChapterId(chapterId));

        OdkAssertions.MemberOf(currentMember, chapterId);

        var currentSubscription = chapterSubscriptions
            .FirstOrDefault(x => x.ChapterSubscription.Id == memberSubscriptionRecord?.ChapterSubscriptionId)
            ?.ChapterSubscription;

        chapterSubscriptions = chapterSubscriptions
            .Where(x => x.ChapterSubscription.IsVisibleToMembers(chapterPaymentSettings, sitePaymentSettings))
            .ToArray();

        var externalSubscription = await GetExternalSubscription(
            sitePaymentSettings,
            memberSubscriptionRecord,
            chapterSubscriptions.Select(x => x.ChapterSubscription).ToArray());

        return new SubscriptionsPageViewModel
        {
            Chapter = chapter,
            ChapterPaymentSettings = chapterPaymentSettings,
            ChapterSubscriptions = chapterSubscriptions,
            CurrentSubscription = currentSubscription,
            ExternalSubscription = externalSubscription,
            MembershipSettings = membershipSettings,
            MemberSubscription = memberSubscription,
            Platform = platform
        };
    }

    public Task<IReadOnlyCollection<ChapterPage>> GetChapterPages(Guid chapterId)
        => _unitOfWork.ChapterPageRepository.GetByChapterId(chapterId).Run();

    public async Task<IReadOnlyCollection<ChapterQuestion>> GetChapterQuestions(Guid chapterId)
    {
        var questions = await _unitOfWork.ChapterQuestionRepository.GetByChapterId(chapterId).Run();
        return questions
            .OrderBy(x => x.DisplayOrder)
            .ToArray();
    }

    public async Task<IReadOnlyCollection<Chapter>> GetChaptersByOwnerId(Guid ownerId)
    {
        return await _unitOfWork.ChapterRepository.GetByOwnerId(ownerId).Run();
    }

    public async Task<ChaptersHomePageViewModel> GetChaptersDto(PlatformType platform)
    {
        var (chapters, countries) = await _unitOfWork.RunAsync(
            x => platform == PlatformType.Default
                ? x.ChapterRepository.GetAll()
                : x.ChapterRepository.GetByPlatform(platform),
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

    public async Task<IReadOnlyCollection<Chapter>> GetMemberChapters(Guid memberId)
        => await _unitOfWork.ChapterRepository.GetByMemberId(memberId).Run();

    public async Task<bool> NameIsAvailable(string name)
    {
        var existing = await _unitOfWork.ChapterRepository.GetByName(name).Run();
        return existing == null;
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