using System.Web;
using ODK.Core.Chapters;
using ODK.Core.Emails;
using ODK.Core.Exceptions;
using ODK.Data.Core;
using ODK.Services.Authorization;
using ODK.Services.Caching;
using ODK.Services.Emails;
using ODK.Services.Exceptions;
using ODK.Services.Recaptcha;

namespace ODK.Services.Chapters;

public class ChapterService : IChapterService
{
    private readonly IAuthorizationService _authorizationService;
    private readonly ICacheService _cacheService;
    private readonly IEmailService _emailService;
    private readonly IRecaptchaService _recaptchaService;
    private readonly IRequestCache _requestCache;
    private readonly IUnitOfWork _unitOfWork;
    
    public ChapterService(IUnitOfWork unitOfWork, ICacheService cacheService, IEmailService emailService,
        IAuthorizationService authorizationService, IRecaptchaService recaptchaService,
        IRequestCache requestCache)
    {
        _authorizationService = authorizationService;
        _cacheService = cacheService;
        _emailService = emailService;
        _recaptchaService = recaptchaService;
        _requestCache = requestCache;
        _unitOfWork = unitOfWork;
    }

    public async Task<Chapter> GetChapter(string name)
    {
        var chapter = await _unitOfWork.ChapterRepository.GetByName(name).RunAsync();
        if (chapter == null)
        {
            throw new OdkNotFoundException();
        }
        return chapter;
    }

    public async Task<ChapterLinks?> GetChapterLinks(Guid chapterId)
    {
        return await _unitOfWork.ChapterLinksRepository.GetByChapterId(chapterId).RunAsync();
    }

    public async Task<ChapterPaymentSettings?> GetChapterPaymentSettings(Guid currentMemberId, Guid chapterId)
    {
        await _authorizationService.AssertMemberIsChapterMemberAsync(currentMemberId, chapterId);
        return await _unitOfWork.ChapterPaymentSettingsRepository.GetByChapterId(chapterId).RunAsync();
    }
    
    public async Task<ChapterMemberPropertiesDto> GetChapterMemberPropertiesDto(Guid? currentMemberId, Guid chapterId)
    {        
        var (chapterProperties, chapterPropertyOptions, memberProperties, membershipSettings, siteSettings) = await _unitOfWork.RunAsync(
            x => x.ChapterPropertyRepository.GetByChapterId(chapterId),
            x => x.ChapterPropertyOptionRepository.GetByChapterId(chapterId),
            x => x.MemberPropertyRepository.GetByMemberId(currentMemberId ?? Guid.Empty),
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

    public async Task<ChapterMemberSubscriptionsDto> GetChapterMemberSubscriptionsDto(Guid currentMemberId, Guid chapterId)
    {
        var (memberSubscription, chapterSubscriptions, country, paymentSettings, membershipSettings) = await _unitOfWork.RunAsync(
            x => x.MemberSubscriptionRepository.GetByMemberId(currentMemberId),
            x => x.ChapterSubscriptionRepository.GetByChapterId(chapterId),
            x => x.CountryRepository.GetByChapterId(chapterId),
            x => x.ChapterPaymentSettingsRepository.GetByChapterId(chapterId),
            x => x.ChapterMembershipSettingsRepository.GetByChapterId(chapterId));

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

    public Task<ChaptersDto> GetChaptersDto() => _requestCache.GetChaptersDtoAsync();

    public Task<ChapterTexts> GetChapterTexts(Guid chapterId)
    {
        return _unitOfWork.ChapterTextsRepository.GetByChapterId(chapterId).RunAsync();
    }

    public async Task SendContactMessage(Chapter chapter, string fromAddress, string message, string recaptchaToken)
    {
        if (string.IsNullOrWhiteSpace(fromAddress) || string.IsNullOrWhiteSpace(message))
        {
            throw new OdkServiceException("Email address and message must be provided");
        }

        if (!MailUtils.ValidEmailAddress(fromAddress))
        {
            throw new OdkServiceException("Invalid email address format");
        }

        var recaptchaResponse = await _recaptchaService.Verify(recaptchaToken);
        if (!_recaptchaService.Success(recaptchaResponse))
        {
            message = $"[FLAGGED AS SPAM: {recaptchaResponse.Score} / 1.0] {message}";
        }

        _unitOfWork.ContactRequestRepository.Add(new ContactRequest
        {
            ChapterId = chapter.Id,
            CreatedDate = DateTime.UtcNow,
            FromAddress = fromAddress,
            Message = message,
            Sent = false
        });
        await _unitOfWork.SaveChangesAsync();

        var parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            {"from", fromAddress},
            {"message", HttpUtility.HtmlEncode(message)}
        };

        await _emailService.SendContactEmail(chapter, fromAddress, message, parameters);
    }
}
