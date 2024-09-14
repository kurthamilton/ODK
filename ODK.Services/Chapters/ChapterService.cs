using ODK.Core;
using ODK.Core.Chapters;
using ODK.Core.Platforms;
using ODK.Core.Web;
using ODK.Data.Core;
using ODK.Services.Authorization;
using ODK.Services.Caching;
using ODK.Services.Chapters.ViewModels;
using ODK.Services.Emails;
using ODK.Services.Members.ViewModels;

namespace ODK.Services.Chapters;

public class ChapterService : IChapterService
{
    private readonly IAuthorizationService _authorizationService;
    private readonly ICacheService _cacheService;
    private readonly IEmailService _emailService;
    private readonly IHttpRequestProvider _httpRequestProvider;
    private readonly IHtmlSanitizer _htmlSanitizer;
    private readonly IPlatformProvider _platformProvider;
    private readonly IUnitOfWork _unitOfWork;
    
    public ChapterService(
        IUnitOfWork unitOfWork, 
        ICacheService cacheService, 
        IAuthorizationService authorizationService,
        IPlatformProvider platformProvider,
        IEmailService emailService,
        IHttpRequestProvider httpRequestProvider,
        IHtmlSanitizer htmlSanitizer)
    {
        _authorizationService = authorizationService;
        _cacheService = cacheService;
        _emailService = emailService;
        _httpRequestProvider = httpRequestProvider;
        _htmlSanitizer = htmlSanitizer;
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

    public async Task<ChapterPaymentSettings?> GetChapterPaymentSettings(Guid currentMemberId, Guid chapterId)
    {
        var (currentMember, paymentSettings) = await _unitOfWork.RunAsync(
            x => x.MemberRepository.GetById(currentMemberId),
            x => x.ChapterPaymentSettingsRepository.GetByChapterId(chapterId));

        OdkAssertions.MemberOf(currentMember, chapterId);
        return paymentSettings;
    }
    
    public async Task<SubscriptionsPageViewModel> GetChapterMemberSubscriptionsDto(Guid currentMemberId, Chapter chapter)
    {
        var chapterId = chapter.Id;

        var (currentMember, memberSubscription, chapterSubscriptions, paymentSettings, membershipSettings) = await _unitOfWork.RunAsync(
            x => x.MemberRepository.GetById(currentMemberId),
            x => x.MemberSubscriptionRepository.GetByMemberId(currentMemberId, chapterId),
            x => x.ChapterSubscriptionRepository.GetByChapterId(chapterId),
            x => x.ChapterPaymentSettingsRepository.GetByChapterId(chapterId),
            x => x.ChapterMembershipSettingsRepository.GetByChapterId(chapterId));

        OdkAssertions.MemberOf(currentMember, chapterId);

        return new SubscriptionsPageViewModel
        {
            ChapterSubscriptions = chapterSubscriptions,
            MembershipSettings = membershipSettings ?? new(),
            MemberSubscription = memberSubscription,
            PaymentSettings = paymentSettings
        };
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
}
