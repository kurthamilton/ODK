using ODK.Core;
using ODK.Core.Chapters;
using ODK.Core.Countries;
using ODK.Core.Emails;
using ODK.Core.Members;
using ODK.Core.Platforms;
using ODK.Core.Utils;
using ODK.Core.Web;
using ODK.Data.Core;
using ODK.Data.Core.Deferred;
using ODK.Services.Authorization;
using ODK.Services.Caching;
using ODK.Services.Emails;

namespace ODK.Services.Chapters;

public class ChapterService : IChapterService
{
    private readonly IAuthorizationService _authorizationService;
    private readonly ICacheService _cacheService;
    private readonly IEmailService _emailService;
    private readonly IHttpRequestProvider _httpRequestProvider;
    private readonly IPlatformProvider _platformProvider;
    private readonly IUnitOfWork _unitOfWork;
    
    public ChapterService(
        IUnitOfWork unitOfWork, 
        ICacheService cacheService, 
        IAuthorizationService authorizationService,
        IPlatformProvider platformProvider,
        IEmailService emailService,
        IHttpRequestProvider httpRequestProvider)
    {
        _authorizationService = authorizationService;
        _cacheService = cacheService;
        _emailService = emailService;
        _httpRequestProvider = httpRequestProvider;
        _platformProvider = platformProvider;
        _unitOfWork = unitOfWork;
    }        

    public async Task<ServiceResult<Chapter?>> CreateChapter(Guid currentMemberId, ChapterCreateModel model)
    {
        var now = DateTime.UtcNow;
        var platform = _platformProvider.GetPlatform();

        var (memberSubscription, existing, countries, siteEmailSettings) = await _unitOfWork.RunAsync(
            x => x.MemberSiteSubscriptionRepository.GetByMemberId(currentMemberId),
            x => x.ChapterRepository.GetAll(),
            x => x.CountryRepository.GetAll(),
            x => x.SiteEmailSettingsRepository.Get(platform));

        var memberChapters = existing
            .Where(x => x.OwnerId == currentMemberId)
            .ToArray();

        if (memberSubscription.SiteSubscription.GroupLimit != null && memberChapters.Length >= memberSubscription.SiteSubscription.GroupLimit)
        {
            return ServiceResult<Chapter?>.Failure("You cannot create any more groups");
        }

        if (memberSubscription.ExpiresUtc < now)
        {
            return ServiceResult<Chapter?>.Failure("Your subscription has expired");
        }

        model.Description = model.Description.Trim();
        model.Name = model.Name.Trim();        

        if (existing.Any(x => string.Equals(x.Name, model.Name, StringComparison.InvariantCultureIgnoreCase)))
        {
            return ServiceResult<Chapter?>.Failure($"The name '{model.Name}' is taken");
        }        

        TimeZoneInfo? timeZone = null;
        if (model.TimeZoneId != null)
        {
            if (!TimeZoneInfo.TryFindSystemTimeZoneById(model.TimeZoneId, out timeZone))
            {
                return ServiceResult<Chapter?>.Failure("Invalid time zone");
            }
        }

        var originalSlug = model.Name
            .ToLowerInvariant()
            .Replace(' ', '-');

        var slug = originalSlug;
        int version = 2;
        while (existing.Any(x => string.Equals(x.Slug, slug, StringComparison.InvariantCultureIgnoreCase)))
        {
            slug = $"{originalSlug}-{version++}";
        }

        var chapter = new Chapter
        {
            CreatedUtc = now,
            Description = model.Description,
            Name = model.Name,
            OwnerId = currentMemberId,
            Platform = platform,
            Slug = slug,
            TimeZone = timeZone
        };

        _unitOfWork.ChapterRepository.Add(chapter);

        _unitOfWork.ChapterLocationRepository.Add(new ChapterLocation
        {
            ChapterId = chapter.Id,
            LatLong = model.Location,
            Name = model.LocationName
        });

        _unitOfWork.ChapterAdminMemberRepository.Add(new ChapterAdminMember
        {
            ChapterId = chapter.Id,
            MemberId = currentMemberId,
            ReceiveContactEmails = true,
            ReceiveEventCommentEmails = true,
            ReceiveNewMemberEmails = true,
            SendNewMemberEmails = true
        });

        _unitOfWork.MemberChapterRepository.Add(new MemberChapter
        {
            ChapterId = chapter.Id,
            MemberId = currentMemberId,
            CreatedUtc = now            
        });

        await _unitOfWork.SaveChangesAsync();

        await _emailService.SendMemberEmail(chapter, null, new EmailAddressee(siteEmailSettings.ContactEmailAddress, ""),
            "{title} - New group",
            "<p>A group has just been created</p>" +
            "<p>Name: {chapter.name}</p>" +
            "<p>{chapter.description}</p>" +
            "<p>{url}/superadmin/groups</p>",
            new Dictionary<string, string>
            {
                { "chapter.description", chapter.Description },
                { "url", UrlUtils.BaseUrl(_httpRequestProvider.RequestUrl) }
            });

        return ServiceResult<Chapter?>.Successful(chapter);
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

        var (currentMember, memberSubscription, chapterSubscriptions, paymentSettings, membershipSettings) = await _unitOfWork.RunAsync(
            x => x.MemberRepository.GetById(currentMemberId),
            x => x.MemberSubscriptionRepository.GetByMemberId(currentMemberId, chapterId),
            x => x.ChapterSubscriptionRepository.GetByChapterId(chapterId),
            x => x.ChapterPaymentSettingsRepository.GetByChapterId(chapterId),
            x => x.ChapterMembershipSettingsRepository.GetByChapterId(chapterId));

        OdkAssertions.MemberOf(currentMember, chapterId);

        return new ChapterMemberSubscriptionsDto
        {
            ChapterSubscriptions = chapterSubscriptions,
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

        var platform = _platformProvider.GetPlatform();
        if (platform != PlatformType.Default)
        {
            chapters = chapters
                .Where(x => x.Platform == platform)
                .ToArray();
        }

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
