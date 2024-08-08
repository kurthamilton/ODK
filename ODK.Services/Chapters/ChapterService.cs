using ODK.Core;
using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Data.Core;
using ODK.Data.Core.Deferred;
using ODK.Services.Authorization;
using ODK.Services.Caching;

namespace ODK.Services.Chapters;

public class ChapterService : IChapterService
{
    private readonly IAuthorizationService _authorizationService;
    private readonly ICacheService _cacheService;    
    private readonly IUnitOfWork _unitOfWork;
    
    public ChapterService(IUnitOfWork unitOfWork, ICacheService cacheService, IAuthorizationService authorizationService)
    {
        _authorizationService = authorizationService;
        _cacheService = cacheService;
        _unitOfWork = unitOfWork;
    }
    
    public async Task<ChapterLinks?> GetChapterLinks(Guid chapterId)
    {
        return await _unitOfWork.ChapterLinksRepository.GetByChapterId(chapterId).RunAsync();
    }

    public async Task<ChapterPaymentSettings?> GetChapterPaymentSettings(Guid currentMemberId, Guid chapterId)
    {
        var (currentMember, paymentSettings) = await _unitOfWork.RunAsync(
            x => x.MemberRepository.GetById(currentMemberId),
            x => x.ChapterPaymentSettingsRepository.GetByChapterId(chapterId));

        OdkAssertions.MemberOf(currentMember, chapterId);
        return paymentSettings;
    }
    
    public async Task<ChapterMemberPropertiesDto> GetChapterMemberPropertiesDto(Guid? currentMemberId, Guid chapterId)
    {        
        var (chapterProperties, chapterPropertyOptions, memberProperties, membershipSettings, siteSettings) = await _unitOfWork.RunAsync(
            x => x.ChapterPropertyRepository.GetByChapterId(chapterId),
            x => x.ChapterPropertyOptionRepository.GetByChapterId(chapterId),
            x => currentMemberId != null 
                ? x.MemberPropertyRepository.GetByMemberId(currentMemberId.Value, chapterId)
                : new DefaultDeferredQueryMultiple<MemberProperty>(),
            x => x.ChapterMembershipSettingsRepository.GetByChapterId(chapterId),
            x => x.SiteSettingsRepository.Get());

        return new ChapterMemberPropertiesDto
        {
            ChapterProperties = chapterProperties,
            ChapterPropertyOptions = chapterPropertyOptions,
            MemberProperties = memberProperties,
            MembershipSettings = membershipSettings,
            SiteSettings = siteSettings
        };
    }

    public async Task<ChapterMemberSubscriptionsDto> GetChapterMemberSubscriptionsDto(Guid currentMemberId, Chapter chapter)
    {
        var chapterId = chapter.Id;

        var (currentMember, memberSubscription, chapterSubscriptions, country, paymentSettings, membershipSettings) = await _unitOfWork.RunAsync(
            x => x.MemberRepository.GetById(currentMemberId),
            x => x.MemberSubscriptionRepository.GetByMemberId(currentMemberId, chapterId),
            x => x.ChapterSubscriptionRepository.GetByChapterId(chapterId),
            x => x.CountryRepository.GetById(chapter.CountryId),
            x => x.ChapterPaymentSettingsRepository.GetByChapterId(chapterId),
            x => x.ChapterMembershipSettingsRepository.GetByChapterId(chapterId));

        OdkAssertions.MemberOf(currentMember, chapterId);

        return new ChapterMemberSubscriptionsDto
        {
            ChapterSubscriptions = chapterSubscriptions,
            Country = country,
            MembershipSettings = membershipSettings ?? new(),
            MemberSubscription = memberSubscription,
            PaymentSettings = paymentSettings
        };
    }

    public async Task<IReadOnlyCollection<ChapterQuestion>> GetChapterQuestions(Guid chapterId)
    {
        var questions = await _unitOfWork.ChapterQuestionRepository.GetByChapterId(chapterId).RunAsync();
        return questions
            .OrderBy(x => x.DisplayOrder)
            .ToArray();
    }

    public async Task<ChaptersDto> GetChaptersDto()
    {
        var (chapters, countries) = await _unitOfWork.RunAsync(
            x => x.ChapterRepository.GetAll(),
            x => x.CountryRepository.GetAll());

        return new ChaptersDto
        {
            Chapters = chapters,
            Countries = countries
        };
    }

    public Task<ChapterTexts> GetChapterTexts(Guid chapterId)
    {
        return _unitOfWork.ChapterTextsRepository.GetByChapterId(chapterId).RunAsync();
    }    
}
