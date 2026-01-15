using ODK.Core;
using ODK.Core.Chapters;
using ODK.Core.Countries;
using ODK.Core.DataTypes;
using ODK.Core.Extensions;
using ODK.Core.Features;
using ODK.Core.Members;
using ODK.Core.Notifications;
using ODK.Core.Pages;
using ODK.Core.Payments;
using ODK.Core.Platforms;
using ODK.Core.Subscriptions;
using ODK.Core.Utils;
using ODK.Core.Web;
using ODK.Data.Core;
using ODK.Data.Core.Chapters;
using ODK.Data.Core.Deferred;
using ODK.Services.Caching;
using ODK.Services.Chapters.Models;
using ODK.Services.Chapters.ViewModels;
using ODK.Services.Exceptions;
using ODK.Services.Geolocation;
using ODK.Services.Imaging;
using ODK.Services.Logging;
using ODK.Services.Members;
using ODK.Services.Notifications;
using ODK.Services.Payments;
using ODK.Services.Payments.Models;
using ODK.Services.SocialMedia;
using ODK.Services.Subscriptions;
using ODK.Services.Subscriptions.ViewModels;
using ODK.Services.Topics;
using ODK.Services.Web;

namespace ODK.Services.Chapters;

public class ChapterAdminService : OdkAdminServiceBase, IChapterAdminService
{
    private static readonly IReadOnlyDictionary<PlatformType, IReadOnlyCollection<PageType>> _platformPages =
        new Dictionary<PlatformType, IReadOnlyCollection<PageType>>
        {
            { PlatformType.Default, new[] { PageType.Contact, PageType.Members } },
            { PlatformType.DrunkenKnitwits, new[] { PageType.About, PageType.Contact, PageType.Members } }
        };

    private readonly ICacheService _cacheService;
    private readonly IGeolocationService _geolocationService;
    private readonly IHtmlSanitizer _htmlSanitizer;
    private readonly IImageService _imageService;
    private readonly IInstagramService _instagramService;
    private readonly ILoggingService _loggingService;
    private readonly IMemberEmailService _memberEmailService;
    private readonly INotificationService _notificationService;
    private readonly IPaymentProviderFactory _paymentProviderFactory;
    private readonly IPaymentService _paymentService;
    private readonly ChapterAdminServiceSettings _settings;
    private readonly ISiteSubscriptionService _siteSubscriptionService;
    private readonly ITopicService _topicService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUrlProviderFactory _urlProviderFactory;

    public ChapterAdminService(
        IUnitOfWork unitOfWork,
        ICacheService cacheService,
        IHtmlSanitizer htmlSanitizer,
        IInstagramService instagramService,
        INotificationService notificationService,
        IImageService imageService,
        IMemberEmailService memberEmailService,
        ITopicService topicService,
        ChapterAdminServiceSettings settings,
        ISiteSubscriptionService siteSubscriptionService,
        IUrlProviderFactory urlProviderFactory,
        IPaymentProviderFactory paymentProviderFactory,
        IPaymentService paymentService,
        IGeolocationService geolocationService,
        ILoggingService loggingService)
        : base(unitOfWork)
    {
        _cacheService = cacheService;
        _geolocationService = geolocationService;
        _htmlSanitizer = htmlSanitizer;
        _imageService = imageService;
        _instagramService = instagramService;
        _loggingService = loggingService;
        _memberEmailService = memberEmailService;
        _notificationService = notificationService;
        _paymentProviderFactory = paymentProviderFactory;
        _paymentService = paymentService;
        _settings = settings;
        _siteSubscriptionService = siteSubscriptionService;
        _topicService = topicService;
        _unitOfWork = unitOfWork;
        _urlProviderFactory = urlProviderFactory;
    }

    public async Task<ServiceResult> AddChapterAdminMember(MemberChapterServiceRequest request, Guid memberId)
    {
        var (chapterId, currentMemberId) = (request.ChapterId, request.CurrentMemberId);

        var (chapterAdminMembers, currentMember, member, ownerSubscription) = await _unitOfWork.RunAsync(
            x => x.ChapterAdminMemberRepository.GetByChapterId(chapterId),
            x => x.MemberRepository.GetById(currentMemberId),
            x => x.MemberRepository.GetById(memberId),
            x => x.MemberSiteSubscriptionRepository.GetByChapterId(chapterId));

        AssertMemberIsChapterAdmin(currentMember, chapterId, chapterAdminMembers);

        if (ownerSubscription?.HasFeature(SiteFeatureType.AdminMembers) != true)
        {
            return ServiceResult.Failure("Not permitted");
        }

        var existing = chapterAdminMembers.FirstOrDefault(x => x.MemberId == memberId);
        if (existing != null)
        {
            return ServiceResult.Failure("Member is already a chapter admin");
        }

        var adminMember = new ChapterAdminMember
        {
            ChapterId = chapterId,
            MemberId = member.Id
        };

        _unitOfWork.ChapterAdminMemberRepository.Add(adminMember);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> ApproveChapter(MemberChapterServiceRequest request)
    {
        var (chapter, members) = await GetSuperAdminRestrictedContent(request,
            x => x.ChapterRepository.GetById(request.ChapterId),
            x => x.MemberRepository.GetAllByChapterId(request.ChapterId));

        var owner = members.FirstOrDefault(x => x.Id == chapter.OwnerId);
        OdkAssertions.Exists(owner);

        if (chapter.Approved())
        {
            return ServiceResult.Successful();
        }

        chapter.ApprovedUtc = DateTime.UtcNow;

        _unitOfWork.ChapterRepository.Update(chapter);
        await _unitOfWork.SaveChangesAsync();

        await _memberEmailService.SendGroupApprovedEmail(
            request,
            chapter,
            owner);

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult<Chapter?>> CreateChapter(
        MemberServiceRequest request,
        ChapterCreateModel model)
    {
        var now = DateTime.UtcNow;

        var (currentMemberId, platform) = (request.CurrentMemberId, request.Platform);

        var (
            memberSubscription,
            existing,
            siteEmailSettings
        ) = await _unitOfWork.RunAsync(
            x => x.MemberSiteSubscriptionRepository.GetByMemberId(currentMemberId, platform),
            x => x.ChapterRepository.GetAll(),
            x => x.SiteEmailSettingsRepository.Get(platform));

        var memberChapters = existing
            .Where(x => x.OwnerId == currentMemberId)
            .ToArray();

        var chapterLimit = memberSubscription != null
            ? memberSubscription.SiteSubscription.GroupLimit
            : SiteSubscription.DefaultGroupLimit;

        if (memberChapters.Length >= chapterLimit)
        {
            return ServiceResult<Chapter?>.Failure("You cannot create any more groups");
        }

        if (memberSubscription?.ExpiresUtc < now)
        {
            return ServiceResult<Chapter?>.Failure("Your subscription has expired");
        }

        if (existing.Any(x => string.Equals(x.GetDisplayName(platform), model.Name, StringComparison.OrdinalIgnoreCase)))
        {
            return ServiceResult<Chapter?>.Failure($"The name '{model.Name}' is taken");
        }

        var timeZone = await _geolocationService.GetTimeZoneFromLocation(model.Location);
        var country = await _geolocationService.GetCountryFromLocation(model.Location);

        var image = new ChapterImage();

        var result = UpdateChapterImage(image, model.ImageData);
        if (!result.Success)
        {
            return ServiceResult<Chapter?>.Failure(result.Message ?? string.Empty);
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

        if (country == null)
        {
            var countries = await _unitOfWork.CountryRepository.GetAll().Run();
            country = countries.First(x => x.IsoCode2 == "GB");

            await _loggingService.Error($"Error setting country for group '{model.Name}', choosing UK as default");
        }

        var chapter = new Chapter
        {
            CountryId = country.Id,
            CreatedUtc = now,
            Name = model.Name,
            OwnerId = currentMemberId,
            Platform = platform,
            Slug = slug,
            TimeZone = timeZone
        };

        _unitOfWork.ChapterRepository.Add(chapter);

        var texts = new ChapterTexts
        {
            ChapterId = chapter.Id,
            Description = _htmlSanitizer.Sanitize(model.Description, DefaultHtmlSantizerOptions),
        };
        _unitOfWork.ChapterTextsRepository.Add(texts);

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
            CreatedUtc = now,
            Approved = true
        });

        _unitOfWork.ChapterTopicRepository.AddMany(model.TopicIds.Select(x => new ChapterTopic
        {
            ChapterId = chapter.Id,
            TopicId = x
        }));

        if (country != null)
        {
            _unitOfWork.ChapterPaymentSettingsRepository.Add(new ChapterPaymentSettings
            {
                ChapterId = chapter.Id,
                CurrencyId = country.CurrencyId,
                UseSitePaymentProvider = true
            });
        }

        image.ChapterId = chapter.Id;
        _unitOfWork.ChapterImageRepository.Add(image);

        await _unitOfWork.SaveChangesAsync();

        await _topicService.AddNewChapterTopics(
            MemberChapterServiceRequest.Create(chapter.Id, request),
            model.NewTopics);

        await _memberEmailService.SendNewGroupEmail(
            request,
            chapter,
            texts,
            siteEmailSettings);

        return ServiceResult<Chapter?>.Successful(chapter);
    }

    public async Task<ServiceResult<ChapterPaymentAccount>> CreateChapterPaymentAccount(
        MemberChapterServiceRequest request, string refreshPath, string returnPath)
    {
        var chapterId = request.ChapterId;

        var (
            chapter,
            existing,
            owner,
            ownerSubscription,
            sitePaymentSettings,
            country,
            currency) = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterRepository.GetById(chapterId),
            x => x.ChapterPaymentAccountRepository.GetByChapterId(chapterId),
            x => x.MemberRepository.GetChapterOwner(chapterId),
            x => x.MemberSiteSubscriptionRepository.GetByChapterId(chapterId),
            x => x.SitePaymentSettingsRepository.GetActive(),
            x => x.CountryRepository.GetByChapterId(chapterId),
            x => x.CurrencyRepository.GetByChapterId(chapterId));

        if (ownerSubscription?.HasFeature(SiteFeatureType.Payments) != true)
        {
            return ServiceResult<ChapterPaymentAccount>.Failure("Not permitted");
        }

        if (existing != null)
        {
            return ServiceResult<ChapterPaymentAccount>.Failure("Payment account already exists");
        }

        if (owner == null)
        {
            return ServiceResult<ChapterPaymentAccount>.Failure("Set group owner before you can create a payment account");
        }

        var baseUrl = request.HttpRequestContext.BaseUrl;

        var urlProvider = _urlProviderFactory.Create(request);

        var paymentProvider = _paymentProviderFactory.GetSitePaymentProvider(sitePaymentSettings);

        var result = await paymentProvider.CreateConnectedAccount(new CreateRemoteAccountOptions
        {
            Chapter = chapter,
            ChapterUrl = urlProvider.GroupUrl(chapter),
            Country = country,
            ChapterCurrency = currency,
            Owner = owner
        });

        if (result == null)
        {
            return ServiceResult<ChapterPaymentAccount>.Failure("An error occurred while creating a payment account");
        }

        var onboardingUrl = await paymentProvider.GenerateConnectedAccountSetupUrl(new GenerateRemoteAccountSetupUrlOptions
        {
            Id = result.Id,
            RefreshUrl = UrlUtils.Url(baseUrl, refreshPath),
            ReturnUrl = UrlUtils.Url(baseUrl, returnPath)
        });

        var paymentAccount = new ChapterPaymentAccount
        {
            ChapterId = request.ChapterId,
            CreatedUtc = DateTime.UtcNow,
            ExternalId = result.Id,
            OnboardingUrl = onboardingUrl,
            OwnerId = owner.Id,
            SitePaymentSettingId = sitePaymentSettings.Id
        };
        _unitOfWork.ChapterPaymentAccountRepository.Add(paymentAccount);

        await _unitOfWork.SaveChangesAsync();

        return ServiceResult<ChapterPaymentAccount>.Successful(paymentAccount);
    }

    public async Task<ServiceResult> CreateChapterProperty(MemberChapterServiceRequest request,
        CreateChapterProperty model)
    {
        var chapterId = request.ChapterId;

        var properties = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterPropertyRepository.GetByChapterId(chapterId));

        var displayOrder = properties.Count > 0 ? properties.Max(x => x.DisplayOrder) + 1 : 1;

        var property = new ChapterProperty
        {
            ApplicationOnly = model.ApplicationOnly,
            ChapterId = chapterId,
            DataType = model.DataType,
            DisplayName = model.DisplayName,
            DisplayOrder = displayOrder,
            HelpText = model.HelpText,
            Label = model.Label,
            Name = model.Name.ToLowerInvariant(),
            Required = model.Required,
            Subtitle = model.Subtitle
        };

        var validationResult = ValidateChapterProperty(property, properties);
        if (!validationResult.Success)
        {
            return validationResult;
        }

        _unitOfWork.ChapterPropertyRepository.Add(property);

        if (property.DataType == DataType.DropDown && model.Options?.Count > 0)
        {
            var options = model.Options
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select((x, i) => new ChapterPropertyOption
                {
                    ChapterPropertyId = property.Id,
                    DisplayOrder = i + 1,
                    Value = x
                })
                .ToArray();
            _unitOfWork.ChapterPropertyOptionRepository.AddMany(options);
        }

        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> CreateChapterQuestion(MemberChapterServiceRequest request,
        CreateChapterQuestion model)
    {
        var chapterId = request.ChapterId;

        var existing = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterQuestionRepository.GetByChapterId(chapterId));

        var displayOrder = existing.Count > 0 ? existing.Max(x => x.DisplayOrder) + 1 : 1;

        var question = new ChapterQuestion
        {
            Answer = _htmlSanitizer.Sanitize(model.Answer, DefaultHtmlSantizerOptions),
            ChapterId = chapterId,
            DisplayOrder = displayOrder,
            Name = model.Name
        };

        var validationResult = ValidateChapterQuestion(question);
        if (!validationResult.Success)
        {
            return validationResult;
        }

        _unitOfWork.ChapterQuestionRepository.Add(question);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> CreateChapterSubscription(
        MemberChapterServiceRequest request,
        CreateChapterSubscription model)
    {
        var chapterId = request.ChapterId;

        var (
            chapter,
            ownerSubscription,
            existing,
            chapterPaymentSettings,
            sitePaymentSettings,
            chapterPaymentAccount,
            currency
        ) = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterRepository.GetById(chapterId),
            x => x.MemberSiteSubscriptionRepository.GetByChapterId(chapterId),
            x => x.ChapterSubscriptionRepository.GetByChapterId(chapterId, includeDisabled: true),
            x => x.ChapterPaymentSettingsRepository.GetByChapterId(chapterId),
            x => x.SitePaymentSettingsRepository.GetActive(),
            x => x.ChapterPaymentAccountRepository.GetByChapterId(chapterId),
            x => x.CurrencyRepository.GetByChapterId(chapterId));

        if (ownerSubscription?.HasFeature(SiteFeatureType.MemberSubscriptions) != true)
        {
            return ServiceResult.Failure("Not permitted");
        }

        if (chapterPaymentAccount == null)
        {
            return ServiceResult.Failure("Payment account not set up");
        }

        var useSitePaymentProvider = chapterPaymentSettings == null || chapterPaymentSettings.UseSitePaymentProvider;

        var subscription = new ChapterSubscription
        {
            Amount = model.Amount,
            ChapterId = chapterId,
            CurrencyId = currency.Id,
            Description = _htmlSanitizer.Sanitize(model.Description, DefaultHtmlSantizerOptions),
            Disabled = model.Disabled,
            Months = model.Months,
            Name = model.Name,
            Recurring = model.Recurring,
            Title = model.Title,
            Type = SubscriptionType.Full,
            SitePaymentSettingId = useSitePaymentProvider
                ? sitePaymentSettings.Id
                : null
        };

        var validationResult = ValidateChapterSubscription(subscription, existing);
        if (!validationResult.Success)
        {
            return validationResult;
        }

        if (useSitePaymentProvider)
        {
            var paymentProvider = _paymentProviderFactory.GetSitePaymentProvider(sitePaymentSettings);

            var platform = request.Platform;
            var productName = chapter.FullName;

            var productId = await paymentProvider.GetProductId(productName);
            if (string.IsNullOrEmpty(productId))
            {
                productId = await paymentProvider.CreateProduct(productName);
            }

            if (productId == null)
            {
                throw new Exception("Error creating product");
            }

            subscription.ExternalProductId = productId;

            var externalId = await paymentProvider.CreateSubscriptionPlan(new ExternalSubscriptionPlan
            {
                Amount = (decimal)subscription.Amount,
                CurrencyCode = currency.Code,
                ExternalId = string.Empty,
                ExternalProductId = productId,
                Frequency = SiteSubscriptionFrequency.Monthly,
                Name = subscription.Name,
                NumberOfMonths = subscription.Months,
                Recurring = model.Recurring
            });

            if (externalId == null)
            {
                throw new Exception("Error creating subscription");
            }

            subscription.ExternalId = externalId;

            if (!model.Disabled)
            {
                await paymentProvider.ActivateSubscriptionPlan(externalId);
            }
        }

        _unitOfWork.ChapterSubscriptionRepository.Add(subscription);

        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> DeleteChapter(MemberChapterServiceRequest request)
    {
        var chapter = await GetSuperAdminRestrictedContent(request,
            x => x.ChapterRepository.GetById(request.ChapterId));

        _unitOfWork.ChapterRepository.Delete(chapter);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> DeleteChapterAdminMember(MemberChapterServiceRequest request,
        Guid memberId)
    {
        var (chapterId, currentMemberId) = (request.ChapterId, request.CurrentMemberId);

        var (chapterAdminMembers, chapter, currentMember, member) = await _unitOfWork.RunAsync(
            x => x.ChapterAdminMemberRepository.GetByChapterId(chapterId),
            x => x.ChapterRepository.GetById(chapterId),
            x => x.MemberRepository.GetById(currentMemberId),
            x => x.MemberRepository.GetById(memberId));

        AssertMemberIsChapterAdmin(currentMember, chapterId, chapterAdminMembers);

        var adminMember = chapterAdminMembers.FirstOrDefault(x => x.MemberId == memberId);
        if (adminMember == null)
        {
            return ServiceResult.Failure("Admin member not found");
        }

        if (member.SuperAdmin)
        {
            return ServiceResult.Failure("Cannot delete a super admin");
        }

        if (chapter.OwnerId == memberId)
        {
            return ServiceResult.Failure("Cannot delete owner");
        }

        _unitOfWork.ChapterAdminMemberRepository.Delete(adminMember);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> DeleteChapterContactMessage(MemberChapterServiceRequest request, Guid id)
    {
        var contactMessage = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterContactMessageRepository.GetById(id));

        OdkAssertions.BelongsToChapter(contactMessage, request.ChapterId);

        _unitOfWork.ChapterContactMessageRepository.Delete(contactMessage);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    public async Task DeleteChapterProperty(MemberChapterServiceRequest request, Guid id)
    {
        var properties = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterPropertyRepository.GetByChapterId(request.ChapterId));

        var property = properties.FirstOrDefault(x => x.Id == id);
        OdkAssertions.Exists(property);

        var displayOrder = 1;
        foreach (var reorder in properties.Where(x => x.Id != id).OrderBy(x => x.DisplayOrder))
        {
            if (reorder.DisplayOrder != displayOrder)
            {
                reorder.DisplayOrder = displayOrder;
                _unitOfWork.ChapterPropertyRepository.Update(reorder);
            }

            displayOrder++;
        }

        _unitOfWork.ChapterPropertyRepository.Delete(property);
        await _unitOfWork.SaveChangesAsync();

        _cacheService.RemoveVersionedCollection<ChapterProperty>(property.ChapterId);
    }

    public async Task DeleteChapterQuestion(MemberChapterServiceRequest request, Guid id)
    {
        var questions = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterQuestionRepository.GetByChapterId(request.ChapterId));

        var question = questions.FirstOrDefault(x => x.Id == id);
        OdkAssertions.Exists(question);

        var displayOrder = 1;
        foreach (var reorder in questions.Where(x => x.Id != id).OrderBy(x => x.DisplayOrder))
        {
            if (reorder.DisplayOrder != displayOrder)
            {
                reorder.DisplayOrder = displayOrder;
                _unitOfWork.ChapterQuestionRepository.Update(reorder);
            }

            displayOrder++;
        }

        _unitOfWork.ChapterQuestionRepository.Delete(question);
        await _unitOfWork.SaveChangesAsync();

        _cacheService.RemoveVersionedCollection<ChapterQuestion>(question.ChapterId);
    }

    public async Task<ServiceResult> DeleteChapterSubscription(MemberChapterServiceRequest request, Guid id)
    {
        var (subscription, inUse) = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterSubscriptionRepository.GetById(id),
            x => x.ChapterSubscriptionRepository.InUse(id));

        if (inUse)
        {
            var message = "Subscriptions cannot be deleted if they have already been used - try disabling instead.";
            return ServiceResult.Failure(message);
        }

        _unitOfWork.ChapterSubscriptionRepository.Delete(subscription);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult<string>> GenerateChapterPaymentAccountSetupUrl(
        MemberChapterServiceRequest request, string refreshPath, string returnPath)
    {
        var existing = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterPaymentAccountRepository.GetByChapterId(request.ChapterId));

        if (existing == null)
        {
            return ServiceResult<string>.Failure("Payment account does not exist");
        }

        var baseUrl = request.HttpRequestContext.BaseUrl;

        var sitePaymentSettings = await _unitOfWork.SitePaymentSettingsRepository
            .GetById(existing.SitePaymentSettingId)
            .Run();

        var paymentProvider = _paymentProviderFactory.GetSitePaymentProvider(sitePaymentSettings);

        var url = await paymentProvider.GenerateConnectedAccountSetupUrl(new GenerateRemoteAccountSetupUrlOptions
        {
            Id = existing.ExternalId,
            RefreshUrl = UrlUtils.Url(baseUrl, refreshPath),
            ReturnUrl = UrlUtils.Url(baseUrl, returnPath)
        });

        if (string.IsNullOrEmpty(url))
        {
            return ServiceResult<string>.Failure("An error occurred while refresh a payment account");
        }

        existing.OnboardingUrl = url;
        _unitOfWork.ChapterPaymentAccountRepository.Update(existing);

        await _unitOfWork.SaveChangesAsync();

        return ServiceResult<string>.Successful(url);
    }

    public async Task<IReadOnlyCollection<ChapterAdminMember>> GetChapterAdminMembers(MemberChapterServiceRequest request)
    {
        var (chapterId, currentMemberId) = (request.ChapterId, request.CurrentMemberId);

        var (chapterAdminMembers, currentMember) = await _unitOfWork.RunAsync(
            x => x.ChapterAdminMemberRepository.GetByChapterId(chapterId),
            x => x.MemberRepository.GetById(currentMemberId));

        AssertMemberIsChapterAdmin(currentMember, chapterId, chapterAdminMembers);

        return chapterAdminMembers;
    }

    public async Task<ChapterAdminPageViewModel> GetChapterAdminPageViewModel(MemberChapterServiceRequest request)
    {
        var platform = request.Platform;

        var chapter = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterRepository.GetById(request.ChapterId));

        return new ChapterAdminPageViewModel
        {
            Chapter = chapter,
            Platform = platform
        };
    }

    public async Task<ChapterConversationsAdminPageViewModel> GetChapterConversationsViewModel(
        MemberChapterServiceRequest request, bool readByChapter)
    {
        var platform = request.Platform;

        var (chapter, privacySettings, conversations) = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterRepository.GetById(request.ChapterId),
            x => x.ChapterPrivacySettingsRepository.GetByChapterId(request.ChapterId),
            x => x.ChapterConversationRepository.GetDtosByChapterId(request.ChapterId, readByChapter: readByChapter));

        var repliedConversations = new List<ChapterConversationDto>();
        var unrepliedConversations = new List<ChapterConversationDto>();

        return new ChapterConversationsAdminPageViewModel
        {
            Chapter = chapter,
            Conversations = conversations,
            Platform = platform,
            PrivacySettings = privacySettings,
            ReadByChapter = readByChapter
        };
    }

    public async Task<ChapterConversationAdminPageViewModel> GetChapterConversationViewModel(
        MemberChapterServiceRequest request, Guid id)
    {
        var platform = request.Platform;

        var (
            chapter,
            ownerSubscription,
            currentMember,
            conversation,
            messages,
            notifications
        ) = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterRepository.GetById(request.ChapterId),
            x => x.MemberSiteSubscriptionRepository.GetByChapterId(request.ChapterId),
            x => x.MemberRepository.GetById(request.CurrentMemberId),
            x => x.ChapterConversationRepository.GetById(id),
            x => x.ChapterConversationMessageRepository.GetByConversationId(id),
            x => x.NotificationRepository.GetUnreadByChapterId(request.ChapterId, NotificationType.ConversationOwnerMessage, id));

        var adminMemberNotifications = notifications
            .Where(x => x.MemberId != conversation.MemberId)
            .ToArray();

        OdkAssertions.BelongsToChapter(conversation, request.ChapterId);

        var (member, otherConversations) = await _unitOfWork.RunAsync(
            x => x.MemberRepository.GetById(conversation.MemberId),
            x => x.ChapterConversationRepository.GetDtosByMemberId(conversation.MemberId, request.ChapterId));

        var lastMessage = messages
            .OrderByDescending(x => x.CreatedUtc)
            .First();

        var canReply = lastMessage.MemberId == conversation.MemberId ||
            ownerSubscription?.HasFeature(SiteFeatureType.SendMemberEmails) == true;

        var unread = messages
            .Where(x => !x.ReadByChapter)
            .ToArray();

        if (unread.Length > 0)
        {
            unread.ForEach(x => x.ReadByChapter = true);
            _unitOfWork.ChapterConversationMessageRepository.UpdateMany(unread);
        }

        if (notifications.Count > 0)
        {
            _unitOfWork.NotificationRepository.MarkAsRead(notifications);
        }

        if (unread.Length > 0 || notifications.Count > 0)
        {
            await _unitOfWork.SaveChangesAsync();
        }

        return new ChapterConversationAdminPageViewModel
        {
            CanReply = canReply,
            Chapter = chapter,
            Conversation = conversation,
            CurrentMember = currentMember,
            Member = member,
            Messages = messages,
            OtherConversations = otherConversations.Where(x => x.Conversation.Id != id).ToArray(),
            OwnerSubscription = ownerSubscription,
            Platform = platform
        };
    }

    public async Task<ChapterDeleteAdminPageViewModel> GetChapterDeleteViewModel(
        MemberChapterServiceRequest request)
    {
        var platform = request.Platform;

        var (chapter, memberCount) = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterRepository.GetById(request.ChapterId),
            x => x.MemberRepository.GetCountByChapterId(request.ChapterId));

        return new ChapterDeleteAdminPageViewModel
        {
            Chapter = chapter,
            MemberCount = memberCount,
            Platform = platform
        };
    }

    public async Task<ChapterImageAdminPageViewModel> GetChapterImageViewModel(MemberChapterServiceRequest request)
    {
        var platform = request.Platform;

        var (chapter, image) = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterRepository.GetById(request.ChapterId),
            x => x.ChapterImageRepository.GetByChapterId(request.ChapterId));

        return new ChapterImageAdminPageViewModel
        {
            Chapter = chapter,
            Image = image,
            Platform = platform
        };
    }

    public async Task<ChapterLinksAdminPageViewModel> GetChapterLinksViewModel(MemberChapterServiceRequest request)
    {
        var platform = request.Platform;

        var (chapter, ownerSubscription, links, privacySettings) = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterRepository.GetById(request.ChapterId),
            x => x.MemberSiteSubscriptionRepository.GetByChapterId(request.ChapterId),
            x => x.ChapterLinksRepository.GetByChapterId(request.ChapterId),
            x => x.ChapterPrivacySettingsRepository.GetByChapterId(request.ChapterId));

        return new ChapterLinksAdminPageViewModel
        {
            Chapter = chapter,
            Links = links,
            OwnerSubscription = ownerSubscription,
            Platform = platform,
            ShowInstagramFeed = privacySettings?.InstagramFeed != null
                ? privacySettings.InstagramFeed.Value
                : !string.IsNullOrEmpty(links?.InstagramName)
        };
    }

    public async Task<ChapterLocationAdminPageViewModel> GetChapterLocationViewModel(
        MemberChapterServiceRequest request)
    {
        var (chapterId, platform) = (request.ChapterId, request.Platform);

        var (chapter, country) = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterRepository.GetById(chapterId),
            x => x.CountryRepository.GetByChapterId(chapterId));

        var location = await _unitOfWork.ChapterLocationRepository.GetByChapterId(chapterId);

        return new ChapterLocationAdminPageViewModel
        {
            Chapter = chapter,
            Country = country,
            Location = location,
            Platform = platform
        };
    }

    public async Task<ChapterMessagesAdminPageViewModel> GetChapterMessagesViewModel(
        MemberChapterServiceRequest request, bool spam)
    {
        var platform = request.Platform;

        var (chapter, allMessages) = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterRepository.GetById(request.ChapterId),
            x => x.ChapterContactMessageRepository.GetByChapterId(request.ChapterId));

        var messagesBySpaminess = new Dictionary<bool, List<ChapterContactMessage>>
        {
            { false, new() },
            { true, new() }
        };

        foreach (var message in allMessages)
        {
            var spamMessage = message.RecaptchaScore < _settings.ContactMessageRecaptchaScoreThreshold;
            messagesBySpaminess[spamMessage].Add(message);
        }

        return new ChapterMessagesAdminPageViewModel
        {
            Chapter = chapter,
            IsSpam = spam,
            MessageCount = messagesBySpaminess[false].Count,
            Messages = messagesBySpaminess[spam],
            Platform = platform,
            SpamMessageCount = messagesBySpaminess[true].Count
        };
    }

    public async Task<ChapterMessageAdminPageViewModel> GetChapterMessageViewModel(
        MemberChapterServiceRequest request, Guid id)
    {
        var platform = request.Platform;

        var (chapter, message, replies) = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterRepository.GetById(request.ChapterId),
            x => x.ChapterContactMessageRepository.GetById(id),
            x => x.ChapterContactMessageReplyRepository.GetByChapterContactMessageId(id));

        OdkAssertions.BelongsToChapter(message, request.ChapterId);

        return new ChapterMessageAdminPageViewModel
        {
            Chapter = chapter,
            Message = message,
            Platform = platform,
            Replies = replies
        };
    }

    public async Task<ChapterPagesAdminPageViewModel> GetChapterPagesViewModel(MemberChapterServiceRequest request)
    {
        var (chapter, chapterPages) = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterRepository.GetById(request.ChapterId),
            x => x.ChapterPageRepository.GetByChapterId(request.ChapterId));

        var chapterPageDictionary = chapterPages
            .ToDictionary(x => x.PageType);

        var allPages = new List<ChapterPage>();

        var allPageTypes = _platformPages[request.Platform];
        foreach (var pageType in allPageTypes)
        {
            chapterPageDictionary.TryGetValue(pageType, out var chapterPage);

            allPages.Add(new ChapterPage
            {
                Hidden = chapterPage?.Hidden ?? false,
                PageType = pageType,
                Title = chapterPage?.Title
            });
        }

        return new ChapterPagesAdminPageViewModel
        {
            Chapter = chapter,
            ChapterPages = allPages
        };
    }

    public async Task<ChapterPaymentAccountAdminPageViewModel> GetChapterPaymentAccountViewModel(
        MemberChapterServiceRequest request)
    {
        var platform = request.Platform;

        var (chapter, ownerSubscription, paymentAccount) = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterRepository.GetById(request.ChapterId),
            x => x.MemberSiteSubscriptionRepository.GetByChapterId(request.ChapterId),
            x => x.ChapterPaymentAccountRepository.GetByChapterId(request.ChapterId));

        var remainingSteps = new List<string>();

        if (paymentAccount == null)
        {
            remainingSteps.Add("Start onboarding");
        }

        if (paymentAccount != null && !paymentAccount.SetupComplete())
        {
            var sitePaymentSettings = await _unitOfWork.SitePaymentSettingsRepository
                .GetById(paymentAccount.SitePaymentSettingId)
                .Run();

            var paymentProvider = _paymentProviderFactory.GetSitePaymentProvider(sitePaymentSettings);
            var remoteAccount = await paymentProvider.GetConnectedAccount(paymentAccount.ExternalId);

            if (remoteAccount?.InitialOnboardingComplete == true)
            {
                paymentAccount.OnboardingCompletedUtc = DateTime.UtcNow;
                _unitOfWork.ChapterPaymentAccountRepository.Update(paymentAccount);
            }
            else
            {
                remainingSteps.Add("Finish onboarding");
            }

            if (remoteAccount?.IdentityDocumentsProvided == true)
            {
                paymentAccount.IdentityDocumentsProvidedUtc = DateTime.UtcNow;
                _unitOfWork.ChapterPaymentAccountRepository.Update(paymentAccount);
            }
            else
            {
                remainingSteps.Add("Provide identity document");
            }

            await _unitOfWork.SaveChangesAsync();
        }

        return new ChapterPaymentAccountAdminPageViewModel
        {
            Chapter = chapter,
            Enabled = paymentAccount?.SetupComplete() == true,
            ExternalId = paymentAccount?.ExternalId,
            HasPermission = ownerSubscription?.HasFeature(SiteFeatureType.Payments) == true,
            OnboardingUrl = paymentAccount?.SetupComplete() != null
                ? paymentAccount?.OnboardingUrl
                : null,
            Platform = platform,
            RemainingSteps = remainingSteps
        };
    }

    public async Task<ChapterPaymentSettingsAdminPageViewModel> GetChapterPaymentSettingsViewModel(
        MemberChapterServiceRequest request)
    {
        var (paymentSettings, currencies) = await GetSuperAdminRestrictedContent(request,
            x => x.ChapterPaymentSettingsRepository.GetByChapterId(request.ChapterId),
            x => x.CurrencyRepository.GetAll());

        return new ChapterPaymentSettingsAdminPageViewModel
        {
            Currencies = currencies,
            PaymentSettings = paymentSettings
        };
    }

    public async Task<ChapterPrivacyAdminPageViewModel> GetChapterPrivacyViewModel(MemberChapterServiceRequest request)
    {
        var platform = request.Platform;

        var (chapter, privacySettings) = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterRepository.GetById(request.ChapterId),
            x => x.ChapterPrivacySettingsRepository.GetByChapterId(request.ChapterId));

        return new ChapterPrivacyAdminPageViewModel
        {
            Chapter = chapter,
            Platform = platform,
            PrivacySettings = privacySettings
        };
    }

    public async Task<ChapterPropertiesAdminPageViewModel> GetChapterPropertiesViewModel(MemberChapterServiceRequest request)
    {
        var platform = request.Platform;

        var (chapter, properties) = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterRepository.GetById(request.ChapterId),
            x => x.ChapterPropertyRepository.GetByChapterId(request.ChapterId));

        return new ChapterPropertiesAdminPageViewModel
        {
            Chapter = chapter,
            Platform = platform,
            Properties = properties
        };
    }

    public async Task<ChapterPropertyAdminPageViewModel> GetChapterPropertyViewModel(
        MemberChapterServiceRequest request,
        Guid propertyId)
    {
        var platform = request.Platform;

        var (chapter, property, options) = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterRepository.GetById(request.ChapterId),
            x => x.ChapterPropertyRepository.GetById(propertyId),
            x => x.ChapterPropertyOptionRepository.GetByPropertyId(propertyId));

        OdkAssertions.BelongsToChapter(property, chapter.Id);

        return new ChapterPropertyAdminPageViewModel
        {
            Chapter = chapter,
            Platform = platform,
            Options = options,
            Property = property
        };
    }

    public async Task<ChapterQuestionsAdminPageViewModel> GetChapterQuestionsViewModel(MemberChapterServiceRequest request)
    {
        var platform = request.Platform;

        var (chapter, questions) = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterRepository.GetById(request.ChapterId),
            x => x.ChapterQuestionRepository.GetByChapterId(request.ChapterId));

        return new ChapterQuestionsAdminPageViewModel
        {
            Chapter = chapter,
            Platform = platform,
            Questions = questions
        };
    }

    public async Task<ChapterQuestionAdminPageViewModel> GetChapterQuestionViewModel(
        MemberChapterServiceRequest request, Guid questionId)
    {
        var platform = request.Platform;

        var (chapter, question) = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterRepository.GetById(request.ChapterId),
            x => x.ChapterQuestionRepository.GetById(questionId));

        OdkAssertions.BelongsToChapter(question, chapter.Id);

        return new ChapterQuestionAdminPageViewModel
        {
            Chapter = chapter,
            Platform = platform,
            Question = question
        };
    }

    public async Task<PaymentStatusType> GetChapterPaymentCheckoutSessionStatus(
        MemberChapterServiceRequest request, string externalSessionId)
    {
        var chapter = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterRepository.GetById(request.ChapterId));

        if (chapter.OwnerId == null)
        {
            throw new OdkServiceException("Chapter owner not found");
        }

        var chapterOwnerRequest = MemberServiceRequest.Create(chapter.OwnerId.Value, request);

        return await _paymentService.GetMemberSitePaymentCheckoutSessionStatus(
            chapterOwnerRequest, externalSessionId);
    }

    public async Task<ChapterSubscriptionAdminPageViewModel> GetChapterSubscriptionViewModel(
        MemberChapterServiceRequest request)
    {
        var (chapterId, platform) = (request.ChapterId, request.Platform);

        var chapter = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterRepository.GetById(chapterId));

        OdkAssertions.Exists(chapter.OwnerId);

        var chapterOwnerRequest = MemberServiceRequest.Create(chapter.OwnerId.Value, request);
        var siteSubscriptionsViewModel = await _siteSubscriptionService.GetSiteSubscriptionsViewModel(
            request, chapter.OwnerId);

        return new ChapterSubscriptionAdminPageViewModel
        {
            Chapter = chapter,
            Platform = platform,
            SiteSubscriptions = siteSubscriptionsViewModel
        };
    }

    public async Task<ChapterTextsAdminPageViewModel> GetChapterTextsViewModel(MemberChapterServiceRequest request)
    {
        var platform = request.Platform;

        var (chapter, texts) = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterRepository.GetById(request.ChapterId),
            x => x.ChapterTextsRepository.GetByChapterId(request.ChapterId));

        return new ChapterTextsAdminPageViewModel
        {
            Chapter = chapter,
            Platform = platform,
            Texts = texts
        };
    }

    public async Task<ChapterTopicsAdminPageViewModel> GetChapterTopicsViewModel(MemberChapterServiceRequest request)
    {
        var platform = request.Platform;

        var (chapter, chapterTopics, topicGroups, topics) = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterRepository.GetById(request.ChapterId),
            x => x.ChapterTopicRepository.GetByChapterId(request.ChapterId),
            x => x.TopicGroupRepository.GetAll(),
            x => x.TopicRepository.GetAll());

        return new ChapterTopicsAdminPageViewModel
        {
            Chapter = chapter,
            ChapterTopics = chapterTopics,
            Platform = platform,
            TopicGroups = topicGroups,
            Topics = topics
        };
    }

    public async Task<MembershipSettingsAdminPageViewModel> GetMembershipSettingsViewModel(MemberChapterServiceRequest request)
    {
        var platform = request.Platform;

        var (chapter, ownerSubscription, membershipSettings) = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterRepository.GetById(request.ChapterId),
            x => x.MemberSiteSubscriptionRepository.GetByChapterId(request.ChapterId),
            x => x.ChapterMembershipSettingsRepository.GetByChapterId(request.ChapterId));

        return new MembershipSettingsAdminPageViewModel
        {
            Chapter = chapter,
            MembershipSettings = membershipSettings,
            OwnerSubscription = ownerSubscription,
            Platform = platform
        };
    }

    public async Task<SuperAdminChaptersViewModel> GetSuperAdminChaptersViewModel(MemberServiceRequest request)
    {
        var (currentMemberId, platform) = (request.CurrentMemberId, request.Platform);

        var (chapters, subscriptions) = await GetSuperAdminRestrictedContent(currentMemberId,
            x => x.ChapterRepository.GetAll(),
            x => x.MemberSiteSubscriptionRepository.GetAllChapterOwnerSubscriptions(platform));

        var subscriptionDictionary = subscriptions
            .GroupBy(x => x.MemberId)
            .ToDictionary(x => x.Key, x => x.ToArray());

        var approved = new List<SuperAdminChaptersRowViewModel>();
        var pending = new List<SuperAdminChaptersRowViewModel>();

        foreach (var chapter in chapters)
        {
            MemberSiteSubscription? chapterSubscription = null;
            if (chapter.OwnerId != null &&
                subscriptionDictionary.TryGetValue(chapter.OwnerId.Value, out var memberSubscriptions))
            {
                chapterSubscription = memberSubscriptions
                    .OrderByDescending(x => x.ExpiresUtc ?? DateTime.MaxValue)
                    .FirstOrDefault();
            }

            var rowViewModel = new SuperAdminChaptersRowViewModel
            {
                CreatedUtc = chapter.CreatedUtc,
                Id = chapter.Id,
                Name = chapter.GetDisplayName(platform),
                PublishedUtc = chapter.PublishedUtc,
                SiteSubscriptionExpiresUtc = chapterSubscription?.ExpiresUtc,
                SiteSubscriptionName = chapterSubscription?.SiteSubscription.Name
            };

            if (chapter.Approved())
            {
                approved.Add(rowViewModel);
            }
            else
            {
                pending.Add(rowViewModel);
            }
        }

        return new SuperAdminChaptersViewModel
        {
            Approved = approved
                .OrderBy(x => x.Name)
                .ToArray(),
            Pending = pending
                .OrderBy(x => x.CreatedUtc)
                .ToArray()
        };
    }

    public async Task<SuperAdminChapterViewModel> GetSuperAdminChapterViewModel(MemberChapterServiceRequest request)
    {
        var (currentMemberId, chapterId, platform) = (request.CurrentMemberId, request.ChapterId, request.Platform);

        var (chapter, subscription, siteSubscriptions) = await GetSuperAdminRestrictedContent(currentMemberId,
            x => x.ChapterRepository.GetById(chapterId),
            x => x.MemberSiteSubscriptionRepository.GetByChapterId(chapterId),
            x => x.SiteSubscriptionRepository.GetAllEnabled(platform));

        return new SuperAdminChapterViewModel
        {
            Chapter = chapter,
            Platform = platform,
            SiteSubscriptions = siteSubscriptions,
            Subscription = subscription
        };
    }

    public async Task<ServiceResult> PublishChapter(MemberChapterServiceRequest request)
    {
        var chapter = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterRepository.GetById(request.ChapterId));

        if (!chapter.CanBePublished())
        {
            return ServiceResult.Failure("This group cannot be published");
        }

        chapter.PublishedUtc = DateTime.UtcNow;
        _unitOfWork.ChapterRepository.Update(chapter);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> ReplyToConversation(
        MemberChapterServiceRequest request,
        Guid conversationId,
        string message)
    {
        var (
            chapter,
            currentMember,
            conversation,
            messages,
            adminMembers,
            ownerSubscription) = await _unitOfWork.RunAsync(
            x => x.ChapterRepository.GetById(request.ChapterId),
            x => x.MemberRepository.GetById(request.CurrentMemberId),
            x => x.ChapterConversationRepository.GetById(conversationId),
            x => x.ChapterConversationMessageRepository.GetByConversationId(conversationId),
            x => x.ChapterAdminMemberRepository.GetByChapterId(request.ChapterId),
            x => x.MemberSiteSubscriptionRepository.GetByChapterId(request.ChapterId));

        OdkAssertions.BelongsToChapter(conversation, chapter.Id);

        AssertMemberIsChapterAdmin(currentMember, request.ChapterId, adminMembers);

        var lastMessage = messages
            .OrderByDescending(x => x.CreatedUtc)
            .First();

        if (lastMessage.MemberId != conversation.MemberId &&
            ownerSubscription?.HasFeature(SiteFeatureType.SendMemberEmails) != true)
        {
            return ServiceResult.Failure("Not permitted");
        }

        var (member, notificationSettings) = await _unitOfWork.RunAsync(
            x => x.MemberRepository.GetById(conversation.MemberId),
            x => x.MemberNotificationSettingsRepository.GetByMemberId(conversation.MemberId, NotificationType.ConversationAdminMessage));

        var conversationMessage = new ChapterConversationMessage
        {
            ChapterConversationId = conversationId,
            CreatedUtc = DateTime.UtcNow,
            MemberId = request.CurrentMemberId,
            ReadByChapter = true,
            Text = message
        };

        _unitOfWork.ChapterConversationMessageRepository.Add(conversationMessage);

        _notificationService.AddNewConversationAdminMessageNotifications(
            conversation,
            member,
            notificationSettings != null ? [notificationSettings] : []);

        await _unitOfWork.SaveChangesAsync();

        await _memberEmailService.SendChapterConversationEmail(
            request,
            chapter,
            conversation,
            conversationMessage,
            [member],
            isReply: true);

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> ReplyToMessage(
        MemberChapterServiceRequest request,
        Guid messageId,
        string message)
    {
        var (chapter, adminMembers, originalMessage) = await _unitOfWork.RunAsync(
            x => x.ChapterRepository.GetById(request.ChapterId),
            x => x.ChapterAdminMemberRepository.GetByChapterId(request.ChapterId),
            x => x.ChapterContactMessageRepository.GetById(messageId));

        var adminMember = adminMembers.FirstOrDefault(x => x.MemberId == request.CurrentMemberId);

        OdkAssertions.Exists(adminMember);
        OdkAssertions.BelongsToChapter(originalMessage, request.ChapterId);

        var sendResult = await _memberEmailService.SendChapterMessageReply(
            request, chapter, originalMessage, message);
        if (!sendResult.Success)
        {
            return sendResult;
        }

        var now = DateTime.UtcNow;

        originalMessage.RepliedUtc = now;
        _unitOfWork.ChapterContactMessageRepository.Update(originalMessage);

        _unitOfWork.ChapterContactMessageReplyRepository.Add(new ChapterContactMessageReply
        {
            ChapterContactMessageId = originalMessage.Id,
            CreatedUtc = now,
            Message = _htmlSanitizer.Sanitize(message, DefaultHtmlSantizerOptions),
            MemberId = request.CurrentMemberId
        });

        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> SetMessageAsReplied(MemberChapterServiceRequest request, Guid messageId)
    {
        var (adminMembers, originalMessage) = await _unitOfWork.RunAsync(
            x => x.ChapterAdminMemberRepository.GetByChapterId(request.ChapterId),
            x => x.ChapterContactMessageRepository.GetById(messageId));

        var adminMember = adminMembers.FirstOrDefault(x => x.MemberId == request.CurrentMemberId);

        OdkAssertions.Exists(adminMember);
        OdkAssertions.BelongsToChapter(originalMessage, request.ChapterId);

        originalMessage.RepliedUtc = DateTime.UtcNow;

        _unitOfWork.ChapterContactMessageRepository.Update(originalMessage);

        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    public async Task SetOwner(MemberChapterServiceRequest request, Guid memberId)
    {
        var (chapterId, currentMemberId) = (request.ChapterId, request.CurrentMemberId);

        var (currentMember, chapter) = await _unitOfWork.RunAsync(
            x => x.MemberRepository.GetById(currentMemberId),
            x => x.ChapterRepository.GetById(chapterId));

        AssertMemberIsSuperAdmin(currentMember);

        chapter.OwnerId = memberId;
        _unitOfWork.ChapterRepository.Update(chapter);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<ServiceResult> StartConversation(
        MemberChapterServiceRequest request,
        Guid memberId,
        string subject,
        string message)
    {
        var (chapter, ownerSubscription, member, notificationSettings) = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterRepository.GetById(request.ChapterId),
            x => x.MemberSiteSubscriptionRepository.GetByChapterId(request.ChapterId),
            x => x.MemberRepository.GetById(memberId),
            x => x.MemberNotificationSettingsRepository.GetByMemberId(memberId, NotificationType.ConversationAdminMessage));

        OdkAssertions.MemberOf(member, request.ChapterId);

        if (!ownerSubscription.HasFeature(SiteFeatureType.SendMemberEmails))
        {
            return ServiceResult.Failure("Not permitted");
        }

        var now = DateTime.UtcNow;

        var conversation = new ChapterConversation
        {
            ChapterId = request.ChapterId,
            CreatedUtc = now,
            MemberId = memberId,
            Subject = subject
        };

        _unitOfWork.ChapterConversationRepository.Add(conversation);

        var conversationMessage = new ChapterConversationMessage
        {
            ChapterConversationId = conversation.Id,
            CreatedUtc = now,
            MemberId = request.CurrentMemberId,
            ReadByChapter = true,
            Text = message
        };

        _unitOfWork.ChapterConversationMessageRepository.Add(conversationMessage);

        _notificationService.AddNewConversationAdminMessageNotifications(
            conversation,
            member,
            notificationSettings != null ? [notificationSettings] : []);

        await _unitOfWork.SaveChangesAsync();

        await _memberEmailService.SendChapterConversationEmail(
            request,
            chapter,
            conversation,
            conversationMessage,
            [member],
            isReply: false);

        return ServiceResult.Successful();
    }

    public async Task<SiteSubscriptionCheckoutViewModel> StartSiteSubscriptionCheckout(
        MemberChapterServiceRequest request, Guid priceId, string returnPath)
    {
        var chapter = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterRepository.GetById(request.ChapterId));

        OdkAssertions.Exists(chapter.OwnerId);

        var ownerRequest = MemberServiceRequest.Create(chapter.OwnerId.Value, request);

        return await _siteSubscriptionService.StartSiteSubscriptionCheckout(ownerRequest, priceId, returnPath);
    }

    public async Task<ServiceResult> UpdateChapterAdminMember(MemberChapterServiceRequest request, Guid memberId,
        UpdateChapterAdminMember model)
    {
        var (chapterId, currentMemberId) = (request.ChapterId, request.CurrentMemberId);

        var (chapterAdminMembers, currentMember) = await _unitOfWork.RunAsync(
            x => x.ChapterAdminMemberRepository.GetByChapterId(chapterId),
            x => x.MemberRepository.GetById(currentMemberId));
        AssertMemberIsChapterAdmin(currentMember, chapterId, chapterAdminMembers);

        var existing = chapterAdminMembers.FirstOrDefault(x => x.MemberId == memberId);
        OdkAssertions.Exists(existing);

        existing.AdminEmailAddress = model.AdminEmailAddress;
        existing.ReceiveContactEmails = model.ReceiveContactEmails;
        existing.ReceiveEventCommentEmails = model.ReceiveEventCommentEmails;
        existing.ReceiveNewMemberEmails = model.ReceiveNewMemberEmails;
        existing.SendNewMemberEmails = model.SendNewMemberEmails;

        _unitOfWork.ChapterAdminMemberRepository.Update(existing);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> UpdateChapterImage(MemberChapterServiceRequest request, UpdateChapterImage model)
    {
        var image = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterImageRepository.GetByChapterId(request.ChapterId));

        image ??= new ChapterImage();

        var result = UpdateChapterImage(image, model.ImageData);
        if (!result.Success)
        {
            return result;
        }

        _unitOfWork.ChapterImageRepository.Upsert(image, request.ChapterId);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    public async Task UpdateChapterLinks(MemberChapterServiceRequest request, UpdateChapterLinks model)
    {
        var (links, privacySettings, instagramPosts) = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterLinksRepository.GetByChapterId(request.ChapterId),
            x => x.ChapterPrivacySettingsRepository.GetByChapterId(request.ChapterId),
            x => x.InstagramPostRepository.GetByChapterId(request.ChapterId));

        links ??= new ChapterLinks();

        var originalInstagramName = links.InstagramName;

        if (model.Facebook != null)
        {
            links.FacebookName = !string.IsNullOrWhiteSpace(model.Facebook) ? model.Facebook : null;
        }

        if (model.Instagram != null)
        {
            links.InstagramName = !string.IsNullOrWhiteSpace(model.Instagram) ? model.Instagram : null;
        }

        if (model.Twitter != null)
        {
            links.TwitterName = !string.IsNullOrWhiteSpace(model.Twitter) ? model.Twitter : null;
        }

        if (model.WhatsApp != null)
        {
            links.WhatsApp = !string.IsNullOrWhiteSpace(model.WhatsApp) ? model.WhatsApp : null;
        }

        _unitOfWork.ChapterLinksRepository.Upsert(links, request.ChapterId);

        if (links.InstagramName != originalInstagramName)
        {
            _unitOfWork.InstagramPostRepository.DeleteMany(instagramPosts);
        }

        if (privacySettings == null && model.InstagramFeed == false)
        {
            privacySettings = new ChapterPrivacySettings();
        }

        if (privacySettings != null)
        {
            privacySettings.InstagramFeed = model.InstagramFeed;
            _unitOfWork.ChapterPrivacySettingsRepository.Upsert(privacySettings, request.ChapterId);
        }

        await _unitOfWork.SaveChangesAsync();

        if (links.InstagramName != originalInstagramName && !string.IsNullOrEmpty(links.InstagramName))
        {
            try
            {
                await _instagramService.ScrapeLatestInstagramPosts(request.ChapterId);
            }
            catch
            {
                // do nothing
            }
        }
    }

    public async Task<ServiceResult> UpdateChapterCurrency(MemberChapterServiceRequest request, Guid currencyId)
    {
        var chapterId = request.ChapterId;

        var chapter = await _unitOfWork.ChapterRepository.GetById(chapterId).Run();

        var (chapterPaymentSettings, ownerPaymentSettings) = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterPaymentSettingsRepository.GetByChapterId(chapterId),
            x => chapter.OwnerId != null
                ? x.MemberPaymentSettingsRepository.GetByMemberId(chapter.OwnerId.Value)
                : new DefaultDeferredQuerySingleOrDefault<MemberPaymentSettings>());

        chapterPaymentSettings ??= new ChapterPaymentSettings
        {
            UseSitePaymentProvider = true
        };

        chapterPaymentSettings.CurrencyId = currencyId;

        if (chapterPaymentSettings.ChapterId == default)
        {
            chapterPaymentSettings.ChapterId = chapter.Id;
            _unitOfWork.ChapterPaymentSettingsRepository.Add(chapterPaymentSettings);
        }
        else
        {
            _unitOfWork.ChapterPaymentSettingsRepository.Update(chapterPaymentSettings);
        }

        if (ownerPaymentSettings == null && chapter.OwnerId != null)
        {
            _unitOfWork.MemberPaymentSettingsRepository.Add(new MemberPaymentSettings
            {
                CurrencyId = currencyId,
                MemberId = chapter.OwnerId.Value
            });
        }

        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> UpdateChapterDescription(MemberChapterServiceRequest request, string description)
    {
        var texts = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterTextsRepository.GetByChapterId(request.ChapterId));

        texts ??= new ChapterTexts();

        texts.Description = _htmlSanitizer.Sanitize(description, DefaultHtmlSantizerOptions);

        if (texts.ChapterId == default)
        {
            texts.ChapterId = request.ChapterId;
            _unitOfWork.ChapterTextsRepository.Add(texts);
        }
        else
        {
            _unitOfWork.ChapterTextsRepository.Update(texts);
        }

        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> UpdateChapterLocation(MemberChapterServiceRequest request,
        LatLong? location, string? name)
    {
        var chapterId = request.ChapterId;

        var chapter = await GetSuperAdminRestrictedContent(request,
            x => x.ChapterRepository.GetById(request.ChapterId));

        var chapterLocation = await _unitOfWork.ChapterLocationRepository.GetByChapterId(request.ChapterId);

        if (location == null || string.IsNullOrEmpty(name))
        {
            return ServiceResult.Failure("Location not set");
        }

        chapterLocation ??= new ChapterLocation();

        chapterLocation.LatLong = location.Value;
        chapterLocation.Name = name;

        var timeZone = await _geolocationService.GetTimeZoneFromLocation(location.Value);
        var country = await _geolocationService.GetCountryFromLocation(location.Value);

        if (timeZone == null || country == null)
        {
            return ServiceResult.Failure("Country not found");
        }

        chapter.CountryId = country.Id;
        chapter.TimeZone = timeZone;

        _unitOfWork.ChapterRepository.Update(chapter);

        if (chapterLocation.ChapterId == default)
        {
            chapterLocation.ChapterId = chapter.Id;
            _unitOfWork.ChapterLocationRepository.Add(chapterLocation);
        }
        else
        {
            _unitOfWork.ChapterLocationRepository.Update(chapterLocation);
        }

        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> UpdateChapterMembershipSettings(MemberChapterServiceRequest request,
        UpdateChapterMembershipSettings model)
    {
        var (settings, ownerSubscription) = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterMembershipSettingsRepository.GetByChapterId(request.ChapterId),
            x => x.MemberSiteSubscriptionRepository.GetByChapterId(request.ChapterId));

        if (ownerSubscription?.HasFeature(SiteFeatureType.MemberSubscriptions) != true)
        {
            return ServiceResult.Failure("Not permitted");
        }

        settings ??= new ChapterMembershipSettings();

        if (ownerSubscription?.HasFeature(SiteFeatureType.ApproveMembers) == true)
        {
            settings.ApproveNewMembers = model.ApproveNewMembers;
        }

        if (ownerSubscription?.HasFeature(SiteFeatureType.MemberSubscriptions) == true)
        {
            settings.Enabled = model.Enabled;
            settings.MembershipDisabledAfterDaysExpired = model.MembershipDisabledAfterDaysExpired;
            settings.MembershipExpiringWarningDays = model.MembershipExpiringWarningDays;
        }

        settings.TrialPeriodMonths = model.TrialPeriodMonths;

        var validationResult = ValidateChapterMembershipSettings(settings);
        if (!validationResult.Success)
        {
            return validationResult;
        }

        if (settings.ChapterId == default)
        {
            settings.ChapterId = request.ChapterId;
            _unitOfWork.ChapterMembershipSettingsRepository.Add(settings);
        }
        else
        {
            _unitOfWork.ChapterMembershipSettingsRepository.Update(settings);
        }

        await _unitOfWork.SaveChangesAsync();

        _cacheService.UpdateItem(settings, request.ChapterId);

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> UpdateChapterPages(MemberChapterServiceRequest request, UpdateChapterPages model)
    {
        var chapterPages = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterPageRepository.GetByChapterId(request.ChapterId));

        var chapterPageDictionary = chapterPages
            .ToDictionary(x => x.PageType);

        var allPageTypes = _platformPages[request.Platform];

        foreach (var pageUpdate in model.Pages)
        {
            if (!Enum.IsDefined(pageUpdate.Type) || pageUpdate.Type == PageType.None)
            {
                await _loggingService.Warn($"Invalid page type when updating: {pageUpdate.Type}");
                continue;
            }

            chapterPageDictionary.TryGetValue(pageUpdate.Type, out var page);

            if (!allPageTypes.Contains(pageUpdate.Type))
            {
                continue;
            }

            var hasValues = pageUpdate.Hidden || !string.IsNullOrEmpty(pageUpdate.Title);

            if (!hasValues && page == null)
            {
                // nothing to do
                continue;
            }

            if (!hasValues && page != null)
            {
                _unitOfWork.ChapterPageRepository.Delete(page);
                continue;
            }

            page ??= new ChapterPage
            {
                ChapterId = request.ChapterId,
                PageType = pageUpdate.Type,
            };

            page.Hidden = pageUpdate.Hidden;
            page.Title = pageUpdate.Title;

            _unitOfWork.ChapterPageRepository.Upsert(page);
        }

        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> UpdateChapterPaymentSettings(
        MemberChapterServiceRequest request,
        UpdateChapterPaymentSettings model)
    {
        var chapterId = request.ChapterId;

        var settings = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterPaymentSettingsRepository.GetByChapterId(chapterId));

        settings ??= new ChapterPaymentSettings();

        settings.ApiPublicKey = model.ApiPublicKey;
        settings.ApiSecretKey = model.ApiSecretKey;
        settings.CurrencyId = model.CurrencyId;
        settings.Provider = model.Provider;
        settings.UseSitePaymentProvider = model.UseSitePaymentProvider;

        if (settings.Provider == null && !settings.UseSitePaymentProvider)
        {
            return ServiceResult.Failure("Provider must be set if not using site payment provider");
        }

        if (settings.ChapterId == default)
        {
            settings.ChapterId = chapterId;
            _unitOfWork.ChapterPaymentSettingsRepository.Add(settings);
        }
        else
        {
            _unitOfWork.ChapterPaymentSettingsRepository.Update(settings);
        }

        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> UpdateChapterPrivacySettings(
        MemberChapterServiceRequest request,
        UpdateChapterPrivacySettings model)
    {
        var settings = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterPrivacySettingsRepository.GetByChapterId(request.ChapterId));

        settings ??= new ChapterPrivacySettings();

        settings.Conversations = model.Conversations;
        settings.EventResponseVisibility = model.EventResponseVisibility == null || model.EventResponseVisibility.Value.IsMember()
            ? model.EventResponseVisibility
            : null;
        settings.EventVisibility = model.EventVisibility;
        settings.MemberVisibility = model.MemberVisibility;
        settings.VenueVisibility = model.VenueVisibility;

        if (settings.ChapterId == default)
        {
            settings.ChapterId = request.ChapterId;
            _unitOfWork.ChapterPrivacySettingsRepository.Add(settings);
        }
        else
        {
            _unitOfWork.ChapterPrivacySettingsRepository.Update(settings);
        }

        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> UpdateChapterProperty(MemberChapterServiceRequest request,
        Guid propertyId, UpdateChapterProperty model)
    {
        var (properties, options) = await GetChapterAdminRestrictedContent(request,
             x => x.ChapterPropertyRepository.GetByChapterId(request.ChapterId),
             x => x.ChapterPropertyOptionRepository.GetByPropertyId(propertyId));

        var property = properties.FirstOrDefault(x => x.Id == propertyId);
        OdkAssertions.Exists(property);

        property.ApplicationOnly = model.ApplicationOnly;
        property.DisplayName = model.DisplayName;
        property.HelpText = model.HelpText;
        property.Label = _htmlSanitizer.Sanitize(model.Label, DefaultHtmlSantizerOptions);
        property.Name = model.Name.ToLowerInvariant();
        property.Required = model.Required;
        property.Subtitle = model.Subtitle;

        var validationResult = ValidateChapterProperty(property, properties);
        if (!validationResult.Success)
        {
            return validationResult;
        }

        if (property.DataType == DataType.DropDown)
        {
            _unitOfWork.ChapterPropertyOptionRepository.DeleteMany(options);

            if (model.Options != null)
            {
                options = model.Options
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select((x, i) => new ChapterPropertyOption
                    {
                        ChapterPropertyId = property.Id,
                        DisplayOrder = i + 1,
                        Value = x
                    })
                    .ToArray();

                _unitOfWork.ChapterPropertyOptionRepository.AddMany(options);
            }
        }

        _unitOfWork.ChapterPropertyRepository.Update(property);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    public async Task<IReadOnlyCollection<ChapterProperty>> UpdateChapterPropertyDisplayOrder(
        MemberChapterServiceRequest request, Guid propertyId, int moveBy)
    {
        var properties = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterPropertyRepository.GetByChapterId(request.ChapterId));

        var property = properties.FirstOrDefault(x => x.Id == propertyId);
        OdkAssertions.Exists(property);

        if (moveBy == 0)
        {
            return properties;
        }

        ChapterProperty? switchWith;
        if (moveBy > 0)
        {
            switchWith = properties
                .Where(x => x.DisplayOrder > property.DisplayOrder)
                .OrderBy(x => x.DisplayOrder)
                .FirstOrDefault();
        }
        else
        {
            switchWith = properties
                .Where(x => x.DisplayOrder < property.DisplayOrder)
                .OrderByDescending(x => x.DisplayOrder)
                .FirstOrDefault();
        }

        if (switchWith == null)
        {
            return properties;
        }

        property = properties.First(x => x.Id == property.Id);

        (switchWith.DisplayOrder, property.DisplayOrder) = (property.DisplayOrder, switchWith.DisplayOrder);

        _unitOfWork.ChapterPropertyRepository.Update(property);
        _unitOfWork.ChapterPropertyRepository.Update(switchWith);
        await _unitOfWork.SaveChangesAsync();

        _cacheService.RemoveVersionedCollection<ChapterProperty>(property.ChapterId);

        return properties.OrderBy(x => x.DisplayOrder).ToArray();
    }

    public async Task<ServiceResult> UpdateChapterQuestion(MemberChapterServiceRequest request, Guid questionId, CreateChapterQuestion model)
    {
        var question = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterQuestionRepository.GetById(questionId));
        OdkAssertions.BelongsToChapter(question, request.ChapterId);

        question.Answer = _htmlSanitizer.Sanitize(model.Answer, DefaultHtmlSantizerOptions);
        question.Name = model.Name;

        var validationResult = ValidateChapterQuestion(question);
        if (!validationResult.Success)
        {
            return validationResult;
        }

        _unitOfWork.ChapterQuestionRepository.Update(question);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    public async Task<IReadOnlyCollection<ChapterQuestion>> UpdateChapterQuestionDisplayOrder(
        MemberChapterServiceRequest request, Guid questionId, int moveBy)
    {
        var questions = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterQuestionRepository.GetByChapterId(request.ChapterId));

        var question = questions.FirstOrDefault(x => x.Id == questionId);
        OdkAssertions.Exists(question);

        if (moveBy == 0)
        {
            return questions;
        }

        ChapterQuestion? switchWith;
        if (moveBy > 0)
        {
            switchWith = questions
                .Where(x => x.DisplayOrder > question.DisplayOrder)
                .OrderBy(x => x.DisplayOrder)
                .FirstOrDefault();
        }
        else
        {
            switchWith = questions
                .Where(x => x.DisplayOrder < question.DisplayOrder)
                .OrderByDescending(x => x.DisplayOrder)
                .FirstOrDefault();
        }

        if (switchWith == null)
        {
            return questions;
        }

        question = questions.First(x => x.Id == question.Id);

        (switchWith.DisplayOrder, question.DisplayOrder) = (question.DisplayOrder, switchWith.DisplayOrder);

        _unitOfWork.ChapterQuestionRepository.Update(question);
        _unitOfWork.ChapterQuestionRepository.Update(switchWith);
        await _unitOfWork.SaveChangesAsync();

        _cacheService.RemoveVersionedCollection<ChapterQuestion>(question.ChapterId);

        return questions.OrderBy(x => x.DisplayOrder).ToArray();
    }

    public async Task UpdateChapterRedirectUrl(MemberChapterServiceRequest request, string? redirectUrl)
    {
        var chapter = await GetSuperAdminRestrictedContent(request,
            x => x.ChapterRepository.GetById(request.ChapterId));

        chapter.RedirectUrl = redirectUrl;

        _unitOfWork.ChapterRepository.Update(chapter);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<ServiceResult> UpdateChapterSubscription(MemberChapterServiceRequest request,
        Guid id, CreateChapterSubscription model)
    {
        var (sitePaymentSettings, subscriptions) = await GetChapterAdminRestrictedContent(request,
            x => x.SitePaymentSettingsRepository.GetAll(),
            x => x.ChapterSubscriptionRepository.GetByChapterId(request.ChapterId, includeDisabled: true));

        var subscription = subscriptions.FirstOrDefault(x => x.Id == id);
        OdkAssertions.Exists(subscription);

        var wasDisabled = subscription.Disabled;

        // subscription.Amount = model.Amount;
        subscription.Description = model.Description;
        subscription.Disabled = model.Disabled;
        // subscription.Months = model.Months;
        subscription.Name = model.Name;
        subscription.Title = model.Title;

        var validationResult = ValidateChapterSubscription(subscription, subscriptions);
        if (!validationResult.Success)
        {
            return validationResult;
        }

        _unitOfWork.ChapterSubscriptionRepository.Update(subscription);
        await _unitOfWork.SaveChangesAsync();

        if (wasDisabled != model.Disabled && !string.IsNullOrEmpty(subscription.ExternalId))
        {
            var paymentProvider = _paymentProviderFactory.GetSitePaymentProvider(
                sitePaymentSettings,
                subscription.SitePaymentSettingId);

            if (model.Disabled)
            {
                await paymentProvider.DeactivateSubscriptionPlan(subscription.ExternalId);
            }
            else
            {
                await paymentProvider.ActivateSubscriptionPlan(subscription.ExternalId);
            }
        }

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> UpdateChapterTexts(MemberChapterServiceRequest request,
        UpdateChapterTexts model)
    {
        var chapterId = request.ChapterId;

        var texts = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterTextsRepository.GetByChapterId(chapterId));

        if (string.IsNullOrWhiteSpace(model.RegisterText) ||
            string.IsNullOrWhiteSpace(model.WelcomeText))
        {
            return ServiceResult.Failure("Some required fields are missing");
        }

        texts ??= new ChapterTexts();

        texts.Description = model.Description != null
            ? _htmlSanitizer.Sanitize(model.Description, DefaultHtmlSantizerOptions)
            : null;
        texts.RegisterText = _htmlSanitizer.Sanitize(model.RegisterText, DefaultHtmlSantizerOptions);
        texts.WelcomeText = _htmlSanitizer.Sanitize(model.WelcomeText, DefaultHtmlSantizerOptions);

        if (texts.ChapterId == default)
        {
            texts.ChapterId = request.ChapterId;
            _unitOfWork.ChapterTextsRepository.Add(texts);
        }
        else
        {
            _unitOfWork.ChapterTextsRepository.Update(texts);
        }

        await _unitOfWork.SaveChangesAsync();

        _cacheService.RemoveVersionedItem<ChapterTexts>(chapterId);

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> UpdateChapterTheme(MemberChapterServiceRequest request, UpdateChapterTheme model)
    {
        var chapter = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterRepository.GetById(request.ChapterId));

        chapter.ThemeBackground = model.Background;
        chapter.ThemeColor = model.Color;

        _unitOfWork.ChapterRepository.Update(chapter);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> UpdateChapterTopics(MemberChapterServiceRequest request,
        IReadOnlyCollection<Guid> topicIds)
    {
        var chapterTopics = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterTopicRepository.GetByChapterId(request.ChapterId));

        var changes = _unitOfWork.ChapterTopicRepository.Merge(chapterTopics, request.ChapterId, topicIds);
        if (changes > 0)
        {
            await _unitOfWork.SaveChangesAsync();
        }

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> UpdateSuperAdminChapter(
        MemberChapterServiceRequest request, SuperAdminChapterUpdateViewModel viewModel)
    {
        var (chapter, subscription) = await GetSuperAdminRestrictedContent(request,
            x => x.ChapterRepository.GetById(request.ChapterId),
            x => x.MemberSiteSubscriptionRepository.GetByChapterId(request.ChapterId));

        if (chapter.OwnerId == null)
        {
            throw new OdkServiceException($"Error updating group '{chapter.Id}': owner not found");
        }

        if (viewModel.SiteSubscriptionId == null)
        {
            throw new OdkServiceException($"Error updating group '{chapter.Id}': subscription not provided");
        }

        subscription ??= new MemberSiteSubscription
        {
            MemberId = chapter.OwnerId.Value,
            SiteSubscriptionId = viewModel.SiteSubscriptionId.Value
        };

        subscription.ExpiresUtc = viewModel.SubscriptionExpiresUtc;

        if (subscription.Id == default)
        {
            subscription.Id = Guid.NewGuid();
            _unitOfWork.MemberSiteSubscriptionRepository.Add(subscription);
        }
        else
        {
            _unitOfWork.MemberSiteSubscriptionRepository.Update(subscription);
        }

        await _unitOfWork.SaveChangesAsync();
        return ServiceResult.Successful();
    }

    private ServiceResult UpdateChapterImage(ChapterImage image, byte[] imageData)
    {
        if (!_imageService.IsImage(imageData))
        {
            return ServiceResult.Failure("Invalid image");
        }

        var mimeType = ChapterImage.DefaultMimeType;

        image.ImageData = _imageService.Process(imageData, new ImageProcessingOptions
        {
            MaxWidth = ChapterImage.MaxWidth,
            MimeType = mimeType
        });
        image.MimeType = mimeType;

        return ServiceResult.Successful();
    }

    private ServiceResult ValidateChapterQuestion(ChapterQuestion question)
    {
        if (string.IsNullOrWhiteSpace(question.Name) ||
            string.IsNullOrWhiteSpace(question.Answer))
        {
            return ServiceResult.Failure("Some required fields are missing");
        }

        return ServiceResult.Successful();
    }

    private ServiceResult ValidateChapterMembershipSettings(ChapterMembershipSettings settings)
    {
        if (settings.MembershipDisabledAfterDaysExpired < 0 ||
            settings.TrialPeriodMonths < 0)
        {
            return ServiceResult.Failure("Some required fields are missing");
        }

        return ServiceResult.Successful();
    }

    private ServiceResult ValidateChapterProperty(ChapterProperty property,
        IReadOnlyCollection<ChapterProperty> existing)
    {
        if (string.IsNullOrEmpty(property.Name) ||
            string.IsNullOrEmpty(property.Label) ||
            !Enum.IsDefined(typeof(DataType), property.DataType) || property.DataType == DataType.None)
        {
            return ServiceResult.Failure("Some required fields are missing");
        }

        if (existing.Any(x => x.Name.Equals(property.Name, StringComparison.OrdinalIgnoreCase) && x.Id != property.Id))
        {
            return ServiceResult.Failure("Name already exists");
        }

        return ServiceResult.Successful();
    }

    private ServiceResult ValidateChapterSubscription(ChapterSubscription subscription,
        IReadOnlyCollection<ChapterSubscription> subscriptions)
    {
        if (!Enum.IsDefined(typeof(SubscriptionType), subscription.Type) || subscription.Type == SubscriptionType.None)
        {
            return ServiceResult.Failure("Invalid type");
        }

        if (string.IsNullOrWhiteSpace(subscription.Description) ||
            string.IsNullOrWhiteSpace(subscription.Name) ||
            string.IsNullOrWhiteSpace(subscription.Title))
        {
            return ServiceResult.Failure("Some required fields are missing");
        }

        if (subscription.Amount < 0)
        {
            return ServiceResult.Failure("Amount cannot be less than 0");
        }

        if (subscription.Months < 1)
        {
            return ServiceResult.Failure("Subscription must be for at least 1 month");
        }

        subscriptions = subscriptions
            .Where(x => x.SitePaymentSettingId == subscription.SitePaymentSettingId)
            .ToArray();

        if (subscriptions.Any(x => x.Id != subscription.Id && x.Name.Equals(subscription.Name)))
        {
            return ServiceResult.Failure("A subscription with that name already exists");
        }

        return ServiceResult.Successful();
    }
}