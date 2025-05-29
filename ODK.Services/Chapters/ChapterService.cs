using ODK.Core;
using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Core.Payments;
using ODK.Core.Platforms;
using ODK.Core.Web;
using ODK.Data.Core;
using ODK.Services.Authorization;
using ODK.Services.Caching;
using ODK.Services.Chapters.ViewModels;
using ODK.Services.Members.ViewModels;
using ODK.Services.Payments;

namespace ODK.Services.Chapters;

public class ChapterService : IChapterService
{
    private readonly IAuthorizationService _authorizationService;
    private readonly ICacheService _cacheService;
    private readonly IHttpRequestProvider _httpRequestProvider;
    private readonly IHtmlSanitizer _htmlSanitizer;
    private readonly IPaymentProviderFactory _paymentProviderFactory;
    private readonly IPlatformProvider _platformProvider;
    private readonly IUnitOfWork _unitOfWork;
    
    public ChapterService(
        IUnitOfWork unitOfWork, 
        ICacheService cacheService, 
        IAuthorizationService authorizationService,
        IPlatformProvider platformProvider,
        IHttpRequestProvider httpRequestProvider,
        IHtmlSanitizer htmlSanitizer,
        IPaymentProviderFactory paymentProviderFactory)
    {
        _authorizationService = authorizationService;
        _cacheService = cacheService;
        _httpRequestProvider = httpRequestProvider;
        _htmlSanitizer = htmlSanitizer;
        _paymentProviderFactory = paymentProviderFactory;
        _platformProvider = platformProvider;
        _unitOfWork = unitOfWork;
    }            

    public async Task<Chapter> GetChapterById(Guid chapterId)
    {
        return await _unitOfWork.ChapterRepository.GetById(chapterId).Run();
    }

    public async Task<Chapter> GetChapterBySlug(string slug)
    {
        var chapter = await _unitOfWork.ChapterRepository.GetBySlug(slug).Run();
        return OdkAssertions.Exists(chapter);
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
    
    public async Task<SubscriptionsPageViewModel> GetChapterMemberSubscriptionsDto(Guid currentMemberId, Chapter chapter)
    {
        var chapterId = chapter.Id;

        var (
            currentMember, 
            memberSubscription, 
            chapterSubscriptions, 
            chapterPaymentSettings, 
            membershipSettings,
            sitePaymentSettings,
            memberSubscriptionRecord
        ) = await _unitOfWork.RunAsync(
            x => x.MemberRepository.GetById(currentMemberId),
            x => x.MemberSubscriptionRepository.GetByMemberId(currentMemberId, chapterId),
            x => x.ChapterSubscriptionRepository.GetByChapterId(chapterId, includeDisabled: false),
            x => x.ChapterPaymentSettingsRepository.GetByChapterId(chapterId),
            x => x.ChapterMembershipSettingsRepository.GetByChapterId(chapterId),
            x => x.SitePaymentSettingsRepository.GetActive(),
            x => x.MemberSubscriptionRecordRepository.GetLatest(currentMemberId, chapterId));

        OdkAssertions.MemberOf(currentMember, chapterId);

        chapterSubscriptions = chapterSubscriptions
            .Where(x => x.Uses(chapterPaymentSettings, sitePaymentSettings))
            .ToArray();

        var externalSubscription = await GetExternalSubscription(
            chapterPaymentSettings, 
            sitePaymentSettings, 
            memberSubscriptionRecord,
            chapterSubscriptions);

        return new SubscriptionsPageViewModel
        {
            ChapterSubscriptions = chapterSubscriptions,
            Currency = chapterPaymentSettings?.Currency,
            ExternalSubscription = externalSubscription,
            MembershipSettings = membershipSettings ?? new(),
            MemberSubscription = memberSubscription,
            PaymentSettings = chapterPaymentSettings?.UseSitePaymentProvider == true
                ? sitePaymentSettings
                : chapterPaymentSettings
        };
    }

    public async Task<IPaymentSettings?> GetChapterPaymentSettings(Guid currentMemberId, Guid chapterId)
    {
        var (currentMember, chapterPaymentSettings, sitePaymentSettings) = await _unitOfWork.RunAsync(
            x => x.MemberRepository.GetById(currentMemberId),
            x => x.ChapterPaymentSettingsRepository.GetByChapterId(chapterId),
            x => x.SitePaymentSettingsRepository.GetActive());

        OdkAssertions.MemberOf(currentMember, chapterId);
        return chapterPaymentSettings?.UseSitePaymentProvider == true
            ? sitePaymentSettings
            : chapterPaymentSettings;
    }

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

    public async Task<ChaptersHomePageViewModel> GetChaptersDto()
    {
        var (chapters, countries) = await _unitOfWork.RunAsync(
            x => x.ChapterRepository.GetAll(),
            x => x.CountryRepository.GetAll());

        var platform = _platformProvider.GetPlatform();
        if (platform != PlatformType.Default)
        {
            chapters = chapters
                .Where(x => x.Platform == platform)
                .ToArray();
        }

        return new ChaptersHomePageViewModel
        {
            Chapters = chapters,
            Countries = countries
        };
    } 

    public async Task<ChapterTexts?> GetChapterTexts(Guid chapterId)
    {
        return await _unitOfWork.ChapterTextsRepository.GetByChapterId(chapterId).Run();
    }

    public async Task<bool> NameIsAvailable(string name)
    {
        var existing = await _unitOfWork.ChapterRepository.GetByName(name).Run();
        return existing == null;
    }

    private async Task<ExternalSubscription?> GetExternalSubscription(
        ChapterPaymentSettings? chapterPaymentSettings,
        SitePaymentSettings sitePaymentSettings,
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

        IPaymentSettings? paymentSettings = chapterSubscription.SitePaymentSettingId != null
            ? chapterSubscription.SitePaymentSettingId == sitePaymentSettings.Id
                ? sitePaymentSettings
                : null
            : chapterPaymentSettings;

        if (paymentSettings == null)
        {
            return null;
        }

        var paymentProvider = _paymentProviderFactory.GetPaymentProvider(paymentSettings);

        return await paymentProvider.GetSubscription(memberSubscriptionRecord.ExternalId);
    }
}
