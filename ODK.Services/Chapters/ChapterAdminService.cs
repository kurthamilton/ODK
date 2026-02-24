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
using ODK.Resources.Resources;
using ODK.Services.Chapters.Models;
using ODK.Services.Chapters.ViewModels;
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
    private static readonly Dictionary<PlatformType, IReadOnlyCollection<PageType>> _platformPages =
        new()
        {
            { PlatformType.Default, new[] { PageType.Contact, PageType.Members } },
            { PlatformType.DrunkenKnitwits, new[] { PageType.About, PageType.Contact, PageType.Members } }
        };

    private readonly IGeolocationService _geolocationService;
    private readonly IHtmlSanitizer _htmlSanitizer;
    private readonly IImageService _imageService;
    private readonly ILoggingService _loggingService;
    private readonly IMemberEmailService _memberEmailService;
    private readonly INotificationService _notificationService;
    private readonly IPaymentProviderFactory _paymentProviderFactory;
    private readonly IPaymentService _paymentService;
    private readonly ChapterAdminServiceSettings _settings;
    private readonly ISiteSubscriptionService _siteSubscriptionService;
    private readonly ISocialMediaService _socialMediaService;
    private readonly ITopicService _topicService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUrlProviderFactory _urlProviderFactory;

    public ChapterAdminService(
        IUnitOfWork unitOfWork,
        IHtmlSanitizer htmlSanitizer,
        ISocialMediaService socialMediaService,
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
        _geolocationService = geolocationService;
        _htmlSanitizer = htmlSanitizer;
        _imageService = imageService;
        _loggingService = loggingService;
        _memberEmailService = memberEmailService;
        _notificationService = notificationService;
        _paymentProviderFactory = paymentProviderFactory;
        _paymentService = paymentService;
        _settings = settings;
        _siteSubscriptionService = siteSubscriptionService;
        _socialMediaService = socialMediaService;
        _topicService = topicService;
        _unitOfWork = unitOfWork;
        _urlProviderFactory = urlProviderFactory;
    }

    public async Task<ServiceResult> AddChapterAdminMember(IMemberChapterAdminServiceRequest request, Guid memberId)
    {
        var (platform, chapterId, currentMemberId) = (request.Platform, request.Chapter.Id, request.CurrentMember.Id);

        var (chapterAdminMembers, currentMember, member, ownerSubscription) = await _unitOfWork.RunAsync(
            x => x.ChapterAdminMemberRepository.GetByChapterId(platform, chapterId),
            x => x.MemberRepository.GetById(currentMemberId),
            x => x.MemberRepository.GetById(memberId),
            x => x.MemberSiteSubscriptionRepository.Query()
                .ForChapterOwner(chapterId)
                .Active()
                .SiteSubscription()
                .GetSingleOrDefault());

        AssertMemberIsChapterAdmin(
            request,
            chapterAdminMembers.FirstOrDefault(x => x.MemberId == currentMemberId));

        if (ownerSubscription?.HasFeature(SiteFeatureType.AdminMembers) != true)
        {
            return ServiceResult.Failure("Not permitted");
        }

        var existing = chapterAdminMembers.FirstOrDefault(x => x.MemberId == memberId);
        if (existing != null)
        {
            return ServiceResult.Failure("Member is already a chapter admin");
        }

        _unitOfWork.ChapterAdminMemberRepository.Add(new ChapterAdminMember
        {
            ChapterId = chapterId,
            MemberId = member.Id,
            Role = ChapterAdminRole.Organiser
        });
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> CancelSiteSubscription(
        IMemberChapterAdminServiceRequest request)
    {
        var (chapter, currentMember) = (request.Chapter, request.CurrentMember);

        var owner = currentMember.Id == chapter.OwnerId
            ? currentMember
            : await GetChapterAdminRestrictedContent(request,
                x => x.MemberRepository.GetChapterOwner(chapter.Id));

        var chapterOwnerRequest = MemberServiceRequest.Create(owner, request);
        return await _siteSubscriptionService.CancelMemberSiteSubscription(chapterOwnerRequest);
    }

    public async Task<ServiceResult<Chapter?>> CreateChapter(
        IMemberServiceRequest request,
        ChapterCreateModel model)
    {
        var now = DateTime.UtcNow;

        var (platform, currentMember) = (request.Platform, request.CurrentMember);

        var name = Chapter.CleanName(model.Name);

        var (
            memberSubscriptionDto,
            memberChapters,
            nameExists,
            siteEmailSettings
        ) = await _unitOfWork.RunAsync(
            x => x.MemberSiteSubscriptionRepository.GetDtoByMemberId(currentMember.Id, platform),
            x => x.ChapterRepository.GetByOwnerId(platform, currentMember.Id),
            x => x.ChapterRepository.NameExists(name),
            x => x.SiteEmailSettingsRepository.Get(platform));

        if (nameExists)
        {
            return ServiceResult<Chapter?>.Failure(
                StringUtils.Interpolate(ErrorMessagesResource.NameTaken, new Dictionary<string, string>
                {
                    { "name", model.Name }
                }));
        }

        var chapterLimit = memberSubscriptionDto != null
            ? memberSubscriptionDto.SiteSubscription.GroupLimit
            : SiteSubscription.DefaultGroupLimit;

        if (memberChapters.Count >= chapterLimit)
        {
            return ServiceResult<Chapter?>.Failure(ErrorMessagesResource.GroupLimitReached);
        }

        if (memberSubscriptionDto?.MemberSiteSubscription.ExpiresUtc < now)
        {
            return ServiceResult<Chapter?>.Failure(ErrorMessagesResource.SubscriptionExpired);
        }

        var timeZone = await _geolocationService.GetTimeZoneFromLocation(model.Location);
        var country = await _geolocationService.GetCountryFromLocation(model.Location);

        var image = new ChapterImage();

        var result = UpdateChapterImage(image, model.ImageData);
        if (!result.Success)
        {
            return ServiceResult<Chapter?>.Failure(result.Message ?? string.Empty);
        }

        if (country == null)
        {
            country = await _unitOfWork.CountryRepository.GetByIsoCode(_settings.DefaultCountryCode).Run();

            await _loggingService.Error(
                $"Error setting country for group '{model.Name}', choosing '{country!.Name}' as default");
        }

        var baseSlug = UrlUtils.Slugify(name);
        var slug = baseSlug;

        for (var i = 2; await _unitOfWork.ChapterRepository.SlugExists(slug).Run(); i++)
        {
            slug = $"{baseSlug}-{i}";
        }

        var chapter = new Chapter
        {
            CountryId = country.Id,
            CreatedUtc = now,
            Name = name,
            OwnerId = currentMember.Id,
            Platform = platform,
            Slug = slug,
            TimeZone = timeZone
        };

        _unitOfWork.ChapterRepository.Add(chapter);

        _unitOfWork.ChapterTextsRepository.Add(new ChapterTexts
        {
            ChapterId = chapter.Id
        });

        _unitOfWork.ChapterLocationRepository.Add(new ChapterLocation
        {
            ChapterId = chapter.Id,
            Latitude = model.Location.Lat,
            Longitude = model.Location.Long,
            Name = model.LocationName
        });

        _unitOfWork.ChapterAdminMemberRepository.Add(new ChapterAdminMember
        {
            ChapterId = chapter.Id,
            MemberId = currentMember.Id,
            ReceiveContactEmails = true,
            ReceiveEventCommentEmails = true,
            ReceiveNewMemberEmails = true,
            Role = ChapterAdminRole.Owner
        });

        _unitOfWork.MemberChapterRepository.Add(new MemberChapter
        {
            ChapterId = chapter.Id,
            MemberId = currentMember.Id,
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
                ChapterId = chapter.Id
            });
        }

        image.ChapterId = chapter.Id;
        _unitOfWork.ChapterImageRepository.Add(image);

        await _unitOfWork.SaveChangesAsync();

        await _topicService.AddNewChapterTopics(
            MemberChapterServiceRequest.Create(chapter, request),
            model.NewTopics);

        await _memberEmailService.SendNewGroupEmail(
            request,
            siteEmailSettings);

        return ServiceResult<Chapter?>.Successful(chapter);
    }

    public async Task<ServiceResult<ChapterPaymentAccount>> CreateChapterPaymentAccount(
        IMemberChapterAdminServiceRequest request, string refreshPath, string returnPath)
    {
        var (chapter, chapterId) = (request.Chapter, request.Chapter.Id);

        var (
            existing,
            owner,
            ownerSubscription,
            sitePaymentSettings,
            country,
            currency) = await GetChapterAdminRestrictedContent(
            request,
            x => x.ChapterPaymentAccountRepository.GetByChapterId(chapterId),
            x => x.MemberRepository.GetChapterOwner(chapterId),
            x => x.MemberSiteSubscriptionRepository.Query()
                .ForChapterOwner(chapterId)
                .Active()
                .SiteSubscription()
                .GetSingleOrDefault(),
            x => x.SitePaymentSettingsRepository.GetActive(),
            x => x.CountryRepository.GetByChapterId(chapterId),
            x => x.CurrencyRepository.GetByChapterId(chapterId));

        if (ownerSubscription?.HasFeature(SiteFeatureType.Payments) != true)
        {
            return ServiceResult<ChapterPaymentAccount>.Failure(ErrorMessagesResource.NotPermitted);
        }

        if (existing != null)
        {
            return ServiceResult<ChapterPaymentAccount>.Failure("Payment account already exists");
        }

        var baseUrl = request.HttpRequestContext.BaseUrl;

        var urlProvider = await _urlProviderFactory.Create(request);

        var paymentProvider = _paymentProviderFactory.GetSitePaymentProvider(sitePaymentSettings);

        var result = await paymentProvider.CreateConnectedAccount(new RemoteAccountCreateOptions
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
            ChapterId = request.Chapter.Id,
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

    public async Task<ServiceResult> CreateChapterProperty(
        IMemberChapterAdminServiceRequest request, ChapterPropertyCreateModel model)
    {
        var chapterId = request.Chapter.Id;

        var properties = await GetChapterAdminRestrictedContent(
            request,
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

    public async Task<ServiceResult> CreateChapterQuestion(
        IMemberChapterAdminServiceRequest request, ChapterQuestionCreateModel model)
    {
        var chapterId = request.Chapter.Id;

        var existing = await GetChapterAdminRestrictedContent(
            request,
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
        IMemberChapterAdminServiceRequest request, ChapterSubscriptionCreateModel model)
    {
        var (chapter, chapterId, currentMember) = (request.Chapter, request.Chapter.Id, request.CurrentMember);

        var (
            ownerSubscription,
            existing,
            chapterPaymentSettings,
            sitePaymentSettings,
            chapterPaymentAccount,
            currency
        ) = await GetChapterAdminRestrictedContent(request,
            x => x.MemberSiteSubscriptionRepository.Query()
                .ForChapterOwner(chapterId)
                .Active()
                .SiteSubscription()
                .GetSingleOrDefault(),
            x => x.ChapterSubscriptionRepository.GetByChapterId(chapterId, includeDisabled: true),
            x => x.ChapterPaymentSettingsRepository.GetByChapterId(chapterId),
            x => x.SitePaymentSettingsRepository.GetActive(),
            x => x.ChapterPaymentAccountRepository.GetByChapterId(chapterId),
            x => x.CurrencyRepository.GetByChapterId(chapterId));

        if (ownerSubscription?.HasFeature(SiteFeatureType.MemberSubscriptions) != true)
        {
            return ServiceResult.Failure(ErrorMessagesResource.NotPermitted);
        }

        if (!currentMember.SiteAdmin)
        {
            if (chapterPaymentAccount == null)
            {
                return ServiceResult.Failure("Payment account not set up");
            }

            if (!chapterPaymentAccount.SetupComplete())
            {
                return ServiceResult.Failure("Payment account set up not finished");
            }
        }

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
            SitePaymentSettingId = sitePaymentSettings.Id
        };

        var validationResult = ValidateChapterSubscription(subscription, existing);
        if (!validationResult.Success)
        {
            return validationResult;
        }

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
        }) ?? throw new Exception("Error creating subscription");

        subscription.ExternalId = externalId;

        if (!model.Disabled)
        {
            await paymentProvider.ActivateSubscriptionPlan(externalId);
        }

        _unitOfWork.ChapterSubscriptionRepository.Add(subscription);

        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> DeleteChapterAdminMember(
        IMemberChapterAdminServiceRequest request, Guid memberId)
    {
        var (platform, chapter, chapterId, currentMember) = (request.Platform, request.Chapter, request.Chapter.Id, request.CurrentMember);

        var chapterAdminMembers = await _unitOfWork.ChapterAdminMemberRepository
            .GetByChapterId(platform, chapterId)
            .Run();

        AssertMemberIsChapterAdmin(
            request,
            chapterAdminMembers.FirstOrDefault(x => x.MemberId == currentMember.Id));

        var adminMember = chapterAdminMembers.FirstOrDefault(x => x.MemberId == memberId);
        if (adminMember == null)
        {
            return ServiceResult.Failure("Admin member not found");
        }

        var member = adminMember.Member;
        if (member.SiteAdmin && !currentMember.SiteAdmin)
        {
            return ServiceResult.Failure("Cannot delete a site admin");
        }

        if (chapter.OwnerId == memberId)
        {
            return ServiceResult.Failure("Cannot delete owner");
        }

        _unitOfWork.ChapterAdminMemberRepository.Delete(adminMember);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> DeleteChapterContactMessage(
        IMemberChapterAdminServiceRequest request, Guid id)
    {
        var contactMessage = await GetChapterAdminRestrictedContent(
            request,
            x => x.ChapterContactMessageRepository.GetById(id));

        OdkAssertions.BelongsToChapter(contactMessage, request.Chapter.Id);

        _unitOfWork.ChapterContactMessageRepository.Delete(contactMessage);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    public async Task DeleteChapterProperty(
        IMemberChapterAdminServiceRequest request, Guid id)
    {
        var properties = await GetChapterAdminRestrictedContent(
            request,
            x => x.ChapterPropertyRepository.GetByChapterId(request.Chapter.Id));

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
    }

    public async Task DeleteChapterQuestion(IMemberChapterAdminServiceRequest request, Guid id)
    {
        var questions = await GetChapterAdminRestrictedContent(
            request,
            x => x.ChapterQuestionRepository.GetByChapterId(request.Chapter.Id));

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
    }

    public async Task<ServiceResult> DeleteChapterSubscription(IMemberChapterAdminServiceRequest request, Guid id)
    {
        var (subscription, inUse) = await GetChapterAdminRestrictedContent(
            request,
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
        IMemberChapterAdminServiceRequest request, string refreshPath, string returnPath)
    {
        var existing = await GetChapterAdminRestrictedContent(
            request,
            x => x.ChapterPaymentAccountRepository.GetByChapterId(request.Chapter.Id));

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

    public async Task<IReadOnlyCollection<ChapterAdminMember>> GetChapterAdminMembers(
        IMemberChapterAdminServiceRequest request)
    {
        var (platform, chapterId, currentMember) = (request.Platform, request.Chapter.Id, request.CurrentMember);

        var chapterAdminMembers = await _unitOfWork.ChapterAdminMemberRepository.GetByChapterId(platform, chapterId).Run();

        AssertMemberIsChapterAdmin(
            request,
            chapterAdminMembers.FirstOrDefault(x => x.MemberId == currentMember.Id));

        return chapterAdminMembers;
    }

    public async Task<ChapterAdminPageViewModel> GetChapterAdminPageViewModel(
        IMemberChapterAdminServiceRequest request)
    {
        await AssertMemberIsChapterAdmin(request);

        return new ChapterAdminPageViewModel
        {
            Chapter = request.Chapter
        };
    }

    public async Task<ChapterConversationsAdminPageViewModel> GetChapterConversationsViewModel(
        IMemberChapterAdminServiceRequest request, bool readByChapter)
    {
        var (platform, chapter) = (request.Platform, request.Chapter);

        var (privacySettings, conversations) = await GetChapterAdminRestrictedContent(
            request,
            x => x.ChapterPrivacySettingsRepository.GetByChapterId(chapter.Id),
            x => x.ChapterConversationRepository.GetDtosByChapterId(chapter.Id, readByChapter: readByChapter));

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
        IMemberChapterAdminServiceRequest request, Guid id)
    {
        var (chapter, currentMember) = (request.Chapter, request.CurrentMember);

        var (
            ownerSubscription,
            conversation,
            messageDtos,
            notifications
        ) = await GetChapterAdminRestrictedContent(
            request,
            x => x.MemberSiteSubscriptionRepository.Query()
                .ForChapterOwner(chapter.Id)
                .Active()
                .SiteSubscription()
                .GetSingleOrDefault(),
            x => x.ChapterConversationRepository.GetById(id),
            x => x.ChapterConversationMessageRepository.GetDtosByConversationId(id),
            x => x.NotificationRepository.GetUnreadByChapterId(chapter.Id, NotificationType.ConversationOwnerMessage, id));

        var adminMemberNotifications = notifications
            .Where(x => x.MemberId != conversation.MemberId)
            .ToArray();

        OdkAssertions.BelongsToChapter(conversation, chapter.Id);

        var (member, otherConversations) = await _unitOfWork.RunAsync(
            x => x.MemberRepository.GetById(conversation.MemberId),
            x => x.ChapterConversationRepository.GetDtosByMemberId(conversation.MemberId, chapter.Id));

        var lastMessage = messageDtos
            .Select(x => x.Message)
            .OrderByDescending(x => x.CreatedUtc)
            .First();

        var canReply = lastMessage.MemberId == conversation.MemberId ||
            ownerSubscription?.HasFeature(SiteFeatureType.SendMemberEmails) == true;

        var unread = messageDtos
            .Select(x => x.Message)
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
            Messages = messageDtos,
            OtherConversations = otherConversations.Where(x => x.Conversation.Id != id).ToArray()
        };
    }

    public async Task<ChapterDeleteAdminPageViewModel> GetChapterDeleteViewModel(
        IMemberChapterAdminServiceRequest request)
    {
        var (platform, chapter) = (request.Platform, request.Chapter);

        var memberCount = await GetChapterAdminRestrictedContent(
            request,
            x => x.MemberRepository.GetCountByChapterId(chapter.Id));

        return new ChapterDeleteAdminPageViewModel
        {
            Chapter = chapter,
            MemberCount = memberCount,
            Platform = platform
        };
    }

    public async Task<ChapterImageAdminPageViewModel> GetChapterImageViewModel(
        IMemberChapterAdminServiceRequest request)
    {
        var chapter = request.Chapter;

        var image = await GetChapterAdminRestrictedContent(
            request,
            x => x.ChapterImageRepository.GetByChapterId(chapter.Id));

        return new ChapterImageAdminPageViewModel
        {
            Chapter = chapter,
            Image = image
        };
    }

    public async Task<ChapterLinksAdminPageViewModel> GetChapterLinksViewModel(
        IMemberChapterAdminServiceRequest request)
    {
        var chapter = request.Chapter;

        var (ownerSubscription, links, privacySettings) = await GetChapterAdminRestrictedContent(
            request,
            x => x.MemberSiteSubscriptionRepository.Query()
                .ForChapterOwner(chapter.Id)
                .Active()
                .SiteSubscription()
                .GetSingleOrDefault(),
            x => x.ChapterLinksRepository.GetByChapterId(chapter.Id),
            x => x.ChapterPrivacySettingsRepository.GetByChapterId(chapter.Id));

        return new ChapterLinksAdminPageViewModel
        {
            Chapter = chapter,
            Links = links,
            OwnerSubscription = ownerSubscription,
            ShowInstagramFeed = privacySettings?.InstagramFeed != null
                ? privacySettings.InstagramFeed.Value
                : !string.IsNullOrEmpty(links?.InstagramName)
        };
    }

    public async Task<ChapterLocationAdminPageViewModel> GetChapterLocationViewModel(
        IMemberChapterAdminServiceRequest request)
    {
        var chapter = request.Chapter;

        var (country, location) = await GetChapterAdminRestrictedContent(
            request,
            x => x.CountryRepository.GetByChapterId(chapter.Id),
            x => x.ChapterLocationRepository.GetByChapterId(chapter.Id));

        return new ChapterLocationAdminPageViewModel
        {
            Chapter = chapter,
            Country = country,
            Location = location
        };
    }

    public async Task<ChapterMessagesAdminPageViewModel> GetChapterMessagesViewModel(
        IMemberChapterAdminServiceRequest request, bool spam)
    {
        var (platform, chapter) = (request.Platform, request.Chapter);

        var allMessages = await GetChapterAdminRestrictedContent(
            request,
            x => x.ChapterContactMessageRepository.GetByChapterId(chapter.Id));

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
        IMemberChapterAdminServiceRequest request, Guid id)
    {
        var chapter = request.Chapter;

        var (message, replies) = await GetChapterAdminRestrictedContent(
            request,
            x => x.ChapterContactMessageRepository.GetById(id),
            x => x.ChapterContactMessageReplyRepository.GetByChapterContactMessageId(id));

        OdkAssertions.BelongsToChapter(message, chapter.Id);

        return new ChapterMessageAdminPageViewModel
        {
            Chapter = chapter,
            Message = message,
            Replies = replies
        };
    }

    public async Task<ChapterPagesAdminPageViewModel> GetChapterPagesViewModel(IMemberChapterAdminServiceRequest request)
    {
        var chapter = request.Chapter;

        var chapterPages = await GetChapterAdminRestrictedContent(
            request,
            x => x.ChapterPageRepository.GetByChapterId(chapter.Id));

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
        IMemberChapterAdminServiceRequest request)
    {
        var (platform, chapter) = (request.Platform, request.Chapter);

        var (ownerSubscription, paymentAccount) = await GetChapterAdminRestrictedContent(
            request,
            x => x.MemberSiteSubscriptionRepository.Query()
                .ForChapterOwner(chapter.Id)
                .Active()
                .SiteSubscription()
                .GetSingleOrDefault(),
            x => x.ChapterPaymentAccountRepository.GetByChapterId(chapter.Id));

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

    public async Task<ChapterPrivacyAdminPageViewModel> GetChapterPrivacyViewModel(
        IMemberChapterAdminServiceRequest request)
    {
        var chapter = request.Chapter;

        var privacySettings = await GetChapterAdminRestrictedContent(
            request,
            x => x.ChapterPrivacySettingsRepository.GetByChapterId(chapter.Id));

        return new ChapterPrivacyAdminPageViewModel
        {
            Chapter = chapter,
            PrivacySettings = privacySettings
        };
    }

    public async Task<ChapterPropertiesAdminPageViewModel> GetChapterPropertiesViewModel(
        IMemberChapterAdminServiceRequest request)
    {
        var (platform, chapter) = (request.Platform, request.Chapter);

        var properties = await GetChapterAdminRestrictedContent(
            request,
            x => x.ChapterPropertyRepository.GetByChapterId(chapter.Id));

        return new ChapterPropertiesAdminPageViewModel
        {
            Chapter = chapter,
            Platform = platform,
            Properties = properties
        };
    }

    public async Task<ChapterPropertyAdminPageViewModel> GetChapterPropertyViewModel(
        IMemberChapterAdminServiceRequest request,
        Guid propertyId)
    {
        var chapter = request.Chapter;

        var (property, options) = await GetChapterAdminRestrictedContent(
            request,
            x => x.ChapterPropertyRepository.GetById(propertyId),
            x => x.ChapterPropertyOptionRepository.GetByPropertyId(propertyId));

        OdkAssertions.BelongsToChapter(property, chapter.Id);

        return new ChapterPropertyAdminPageViewModel
        {
            Options = options,
            Property = property
        };
    }

    public async Task<ChapterQuestionsAdminPageViewModel> GetChapterQuestionsViewModel(
        IMemberChapterAdminServiceRequest request)
    {
        var (platform, chapter) = (request.Platform, request.Chapter);

        var questions = await GetChapterAdminRestrictedContent(
            request,
            x => x.ChapterQuestionRepository.GetByChapterId(chapter.Id));

        return new ChapterQuestionsAdminPageViewModel
        {
            Chapter = chapter,
            Platform = platform,
            Questions = questions
        };
    }

    public async Task<ChapterQuestionAdminPageViewModel> GetChapterQuestionViewModel(
        IMemberChapterAdminServiceRequest request, Guid questionId)
    {
        var chapter = request.Chapter;

        var question = await GetChapterAdminRestrictedContent(
            request,
            x => x.ChapterQuestionRepository.GetById(questionId));

        OdkAssertions.BelongsToChapter(question, chapter.Id);

        return new ChapterQuestionAdminPageViewModel
        {
            Chapter = chapter,
            Question = question
        };
    }

    public async Task<PaymentStatusType> GetChapterPaymentCheckoutSessionStatus(
        IMemberChapterAdminServiceRequest request, string externalSessionId)
    {
        var (platform, chapter, currentMember) = (request.Platform, request.Chapter, request.CurrentMember);

        var owner = chapter.OwnerId == currentMember.Id
            ? currentMember
            : await GetChapterAdminRestrictedContent(request,
                x => x.ChapterRepository.Query(platform)
                    .ById(chapter.Id)
                    .Owner()
                    .GetSingle());

        var ownerRequest = MemberServiceRequest.Create(owner, request);

        return await _paymentService.GetMemberSitePaymentCheckoutSessionStatus(
            ownerRequest, externalSessionId);
    }

    public async Task<SiteSubscriptionsViewModel> GetChapterSubscriptionViewModel(
        IMemberChapterAdminServiceRequest request)
    {
        var (platform, chapter) = (request.Platform, request.Chapter);

        var owner = await GetChapterAdminRestrictedContent(request,
            x => x.MemberRepository.GetChapterOwner(chapter.Id));

        var siteSubscriptionsViewModel = await _siteSubscriptionService.GetSiteSubscriptionsViewModel(
            MemberServiceRequest.Create(owner, request),
            chapter.Id);

        return siteSubscriptionsViewModel;
    }

    public async Task<ChapterTextsAdminPageViewModel> GetChapterTextsViewModel(
        IMemberChapterAdminServiceRequest request)
    {
        var chapter = request.Chapter;

        var texts = await GetChapterAdminRestrictedContent(
            request,
            x => x.ChapterTextsRepository.GetByChapterId(chapter.Id));

        return new ChapterTextsAdminPageViewModel
        {
            Chapter = chapter,
            Texts = texts
        };
    }

    public async Task<ChapterTopicsAdminPageViewModel> GetChapterTopicsViewModel(
        IMemberChapterAdminServiceRequest request)
    {
        var chapter = request.Chapter;

        var (chapterTopics, topicGroups, topics) = await GetChapterAdminRestrictedContent(
            request,
            x => x.ChapterTopicRepository.GetByChapterId(chapter.Id),
            x => x.TopicGroupRepository.GetAll(),
            x => x.TopicRepository.GetAll());

        return new ChapterTopicsAdminPageViewModel
        {
            Chapter = chapter,
            ChapterTopics = chapterTopics,
            TopicGroups = topicGroups,
            Topics = topics
        };
    }

    public async Task<MembershipSettingsAdminPageViewModel> GetMembershipSettingsViewModel(
        IMemberChapterAdminServiceRequest request)
    {
        var chapter = request.Chapter;

        var (ownerSubscription, membershipSettings) = await GetChapterAdminRestrictedContent(
            request,
            x => x.MemberSiteSubscriptionRepository.Query()
                .ForChapterOwner(chapter.Id)
                .Active()
                .SiteSubscription()
                .GetSingleOrDefault(),
            x => x.ChapterMembershipSettingsRepository.GetByChapterId(chapter.Id));

        return new MembershipSettingsAdminPageViewModel
        {
            Chapter = chapter,
            MembershipSettings = membershipSettings,
            OwnerSubscription = ownerSubscription
        };
    }

    public async Task<bool> NameIsAvailable(IServiceRequest request, string name)
    {
        name = Chapter.CleanName(name);

        var nameExists = await _unitOfWork.ChapterRepository
            .NameExists(name)
            .Run();

        return !nameExists;
    }

    public async Task<ServiceResult> PublishChapter(IMemberChapterAdminServiceRequest request)
    {
        var chapter = request.Chapter;

        await AssertMemberIsChapterAdmin(request);

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
        IMemberChapterAdminServiceRequest request,
        Guid conversationId,
        string message)
    {
        var (chapter, currentMember) = (request.Chapter, request.CurrentMember);

        var (
            conversation,
            messages,
            ownerSubscription) = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterConversationRepository.GetById(conversationId),
            x => x.ChapterConversationMessageRepository.GetByConversationId(conversationId),
            x => x.MemberSiteSubscriptionRepository.Query()
                .ForChapterOwner(chapter.Id)
                .Active()
                .SiteSubscription()
                .GetSingleOrDefault());

        OdkAssertions.BelongsToChapter(conversation, chapter.Id);

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
            MemberId = currentMember.Id,
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
            conversation,
            conversationMessage,
            [member],
            isReply: true);

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> ReplyToMessage(
        IMemberChapterAdminServiceRequest request,
        Guid messageId,
        string message)
    {
        var (platform, chapter, currentMember) = (request.Platform, request.Chapter, request.CurrentMember);

        var (adminMembers, originalMessage) = await GetChapterAdminRestrictedContent(
            request,
            x => x.ChapterAdminMemberRepository.GetByChapterId(platform, chapter.Id),
            x => x.ChapterContactMessageRepository.GetById(messageId));

        OdkAssertions.BelongsToChapter(originalMessage, chapter.Id);

        var sendResult = await _memberEmailService.SendChapterMessageReply(
            request,
            originalMessage,
            message);
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
            MemberId = request.CurrentMember.Id
        });

        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> SetMessageAsReplied(
        IMemberChapterAdminServiceRequest request, Guid messageId)
    {
        var chapter = request.Chapter;

        var originalMessage = await GetChapterAdminRestrictedContent(
            request,
            x => x.ChapterContactMessageRepository.GetById(messageId));

        OdkAssertions.BelongsToChapter(originalMessage, chapter.Id);

        originalMessage.RepliedUtc = DateTime.UtcNow;

        _unitOfWork.ChapterContactMessageRepository.Update(originalMessage);

        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    public async Task SetOwner(IMemberChapterAdminServiceRequest request, Guid memberId)
    {
        var (chapter, currentMember) = (request.Chapter, request.CurrentMember);

        AssertMemberIsSiteAdmin(currentMember);

        chapter.OwnerId = memberId;
        _unitOfWork.ChapterRepository.Update(chapter);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<ServiceResult> StartConversation(
        IMemberChapterAdminServiceRequest request,
        Guid memberId,
        string subject,
        string message)
    {
        var chapter = request.Chapter;

        var (ownerSubscription, member, notificationSettings) = await GetChapterAdminRestrictedContent(
            request,
            x => x.MemberSiteSubscriptionRepository.Query()
                .ForChapterOwner(chapter.Id)
                .Active()
                .SiteSubscription()
                .GetSingleOrDefault(),
            x => x.MemberRepository.GetById(memberId),
            x => x.MemberNotificationSettingsRepository.GetByMemberId(memberId, NotificationType.ConversationAdminMessage));

        OdkAssertions.MemberOf(member, chapter.Id);

        if (!ownerSubscription.HasFeature(SiteFeatureType.SendMemberEmails))
        {
            return ServiceResult.Failure("Not permitted");
        }

        var now = DateTime.UtcNow;

        var conversation = new ChapterConversation
        {
            ChapterId = chapter.Id,
            CreatedUtc = now,
            MemberId = memberId,
            Subject = subject
        };

        _unitOfWork.ChapterConversationRepository.Add(conversation);

        var conversationMessage = new ChapterConversationMessage
        {
            ChapterConversationId = conversation.Id,
            CreatedUtc = now,
            MemberId = request.CurrentMember.Id,
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
            conversation,
            conversationMessage,
            [member],
            isReply: false);

        return ServiceResult.Successful();
    }

    public async Task<SiteSubscriptionCheckoutViewModel> StartSiteSubscriptionCheckout(
        IMemberChapterAdminServiceRequest request, Guid priceId, string returnPath)
    {
        var (platform, chapter, currentMember) = (request.Platform, request.Chapter, request.CurrentMember);

        var owner = currentMember.Id == chapter.OwnerId
            ? currentMember
            : await GetChapterAdminRestrictedContent(request,
                x => x.ChapterRepository.Query(platform)
                    .ById(chapter.Id)
                    .Owner()
                    .GetSingle());

        var chapterOwnerRequest = MemberServiceRequest.Create(owner, request);
        return await _siteSubscriptionService.StartSiteSubscriptionCheckout(
            chapterOwnerRequest, priceId, returnPath, chapter.Id);
    }

    public async Task<ServiceResult> UpdateChapterAdminMember(
        IMemberChapterAdminServiceRequest request,
        Guid memberId,
        ChapterAdminMemberUpdateModel model)
    {
        var (platform, chapter, currentMember) = (request.Platform, request.Chapter, request.CurrentMember);

        var chapterAdminMembers = await _unitOfWork.ChapterAdminMemberRepository
            .GetByChapterId(platform, chapter.Id).Run();

        var chapterAdminMember = chapterAdminMembers.FirstOrDefault(x => x.MemberId == currentMember.Id);

        AssertMemberIsChapterAdmin(
            request,
            chapterAdminMember);

        var existing = chapterAdminMembers.FirstOrDefault(x => x.MemberId == memberId);
        OdkAssertions.Exists(existing);

        if (!chapterAdminMember.HasAccessTo(existing.Role, currentMember))
        {
            return ServiceResult.Failure($"You do not have permission to update an admin with the role '{existing.Role}'");
        }

        if (model.Role != existing.Role)
        {
            if (model.Role == ChapterAdminRole.None || !Enum.IsDefined(model.Role))
            {
                return ServiceResult.Failure("Role not set");
            }

            if (existing.Role == ChapterAdminRole.Owner)
            {
                return ServiceResult.Failure("Owner role cannot be changed");
            }
        }

        existing.AdminEmailAddress = model.AdminEmailAddress;
        existing.ReceiveContactEmails = model.ReceiveContactEmails;
        existing.ReceiveEventCommentEmails = model.ReceiveEventCommentEmails;
        existing.ReceiveNewMemberEmails = model.ReceiveNewMemberEmails;
        existing.Role = model.Role;

        _unitOfWork.ChapterAdminMemberRepository.Update(existing);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> UpdateChapterImage(
        IMemberChapterAdminServiceRequest request, ChapterImageUpdateModel model)
    {
        var chapter = request.Chapter;

        var image = await GetChapterAdminRestrictedContent(
            request,
            x => x.ChapterImageRepository.GetByChapterId(chapter.Id));

        image ??= new ChapterImage();

        var result = UpdateChapterImage(image, model.ImageData);
        if (!result.Success)
        {
            return result;
        }

        _unitOfWork.ChapterImageRepository.Upsert(image, chapter.Id);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    public async Task UpdateChapterLinks(IMemberChapterAdminServiceRequest request, ChapterLinksUpdateModel model)
    {
        var chapter = request.Chapter;

        var (links, privacySettings, instagramPosts) = await GetChapterAdminRestrictedContent(
            request,
            x => x.ChapterLinksRepository.GetByChapterId(chapter.Id),
            x => x.ChapterPrivacySettingsRepository.GetByChapterId(chapter.Id),
            x => x.InstagramPostRepository.GetByChapterId(chapter.Id));

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

        _unitOfWork.ChapterLinksRepository.Upsert(links, chapter.Id);

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
            _unitOfWork.ChapterPrivacySettingsRepository.Upsert(privacySettings, chapter.Id);
        }

        await _unitOfWork.SaveChangesAsync();

        if (links.InstagramName != originalInstagramName && !string.IsNullOrEmpty(links.InstagramName))
        {
            try
            {
                await _socialMediaService.ScrapeLatestInstagramPosts(chapter.Id);
            }
            catch
            {
                // do nothing
            }
        }
    }

    public async Task<ServiceResult> UpdateChapterDescription(
        IMemberChapterAdminServiceRequest request, string description)
    {
        var chapter = request.Chapter;

        var texts = await GetChapterAdminRestrictedContent(
            request,
            x => x.ChapterTextsRepository.GetByChapterId(chapter.Id));

        texts ??= new ChapterTexts();

        texts.Description = _htmlSanitizer.Sanitize(description, DefaultHtmlSantizerOptions);

        if (texts.ChapterId == default)
        {
            texts.ChapterId = chapter.Id;
            _unitOfWork.ChapterTextsRepository.Add(texts);
        }
        else
        {
            _unitOfWork.ChapterTextsRepository.Update(texts);
        }

        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> UpdateChapterLocation(
        IMemberChapterAdminServiceRequest request,
        LatLong? location,
        string? name)
    {
        var (chapter, currentMember) = (request.Chapter, request.CurrentMember);

        AssertMemberIsSiteAdmin(currentMember);

        var chapterLocation = await _unitOfWork.ChapterLocationRepository
            .GetByChapterId(chapter.Id)
            .Run();

        if (location == null || string.IsNullOrEmpty(name))
        {
            return ServiceResult.Failure("Location not set");
        }

        chapterLocation ??= new ChapterLocation();

        chapterLocation.Latitude = location.Value.Lat;
        chapterLocation.Longitude = location.Value.Long;
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

    public async Task<ServiceResult> UpdateChapterMembershipSettings(
        IMemberChapterAdminServiceRequest request, ChapterMembershipSettingsUpdateModel model)
    {
        var chapter = request.Chapter;

        var (settings, ownerSubscription) = await GetChapterAdminRestrictedContent(
            request,
            x => x.ChapterMembershipSettingsRepository.GetByChapterId(chapter.Id),
            x => x.MemberSiteSubscriptionRepository.Query()
                .ForChapterOwner(chapter.Id)
                .Active()
                .SiteSubscription()
                .GetSingleOrDefault());

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
            settings.ChapterId = chapter.Id;
            _unitOfWork.ChapterMembershipSettingsRepository.Add(settings);
        }
        else
        {
            _unitOfWork.ChapterMembershipSettingsRepository.Update(settings);
        }

        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> UpdateChapterPages(
        IMemberChapterAdminServiceRequest request, ChapterPagesUpdateModel model)
    {
        var chapter = request.Chapter;

        var chapterPages = await GetChapterAdminRestrictedContent(
            request,
            x => x.ChapterPageRepository.GetByChapterId(chapter.Id));

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
                ChapterId = chapter.Id,
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
        IMemberChapterAdminServiceRequest request,
        ChapterPaymentSettingsUpdateModel model)
    {
        var chapter = request.Chapter;

        var (chapterPaymentSettings, memberPaymentSettings) = await GetChapterAdminRestrictedContent(
            request,
            x => x.ChapterPaymentSettingsRepository.GetByChapterId(chapter.Id),
            x => x.MemberPaymentSettingsRepository.GetByChapterId(chapter.Id));

        chapterPaymentSettings ??= new ChapterPaymentSettings();
        chapterPaymentSettings.CurrencyId = model.CurrencyId;

        if (chapterPaymentSettings.ChapterId == default)
        {
            chapterPaymentSettings.ChapterId = chapter.Id;
            _unitOfWork.ChapterPaymentSettingsRepository.Add(chapterPaymentSettings);
        }
        else
        {
            _unitOfWork.ChapterPaymentSettingsRepository.Update(chapterPaymentSettings);
        }

        if (model.CurrencyId != null)
        {
            memberPaymentSettings ??= new MemberPaymentSettings();
            memberPaymentSettings.CurrencyId = model.CurrencyId.Value;

            if (memberPaymentSettings.MemberId == default)
            {
                memberPaymentSettings.MemberId = chapter.OwnerId;
                _unitOfWork.MemberPaymentSettingsRepository.Add(memberPaymentSettings);
            }
            else
            {
                _unitOfWork.MemberPaymentSettingsRepository.Update(memberPaymentSettings);
            }
        }

        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> UpdateChapterPrivacySettings(
        IMemberChapterAdminServiceRequest request,
        ChapterPrivacySettingsUpdateModel model)
    {
        var chapter = request.Chapter;

        var settings = await GetChapterAdminRestrictedContent(
            request,
            x => x.ChapterPrivacySettingsRepository.GetByChapterId(chapter.Id));

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
            settings.ChapterId = chapter.Id;
            _unitOfWork.ChapterPrivacySettingsRepository.Add(settings);
        }
        else
        {
            _unitOfWork.ChapterPrivacySettingsRepository.Update(settings);
        }

        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> UpdateChapterProperty(
        IMemberChapterAdminServiceRequest request,
        Guid propertyId,
        ChapterPropertyUpdateModel model)
    {
        var chapter = request.Chapter;

        var (properties, options) = await GetChapterAdminRestrictedContent(
            request,
            x => x.ChapterPropertyRepository.GetByChapterId(chapter.Id),
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
        IMemberChapterAdminServiceRequest request, Guid propertyId, int moveBy)
    {
        var chapter = request.Chapter;

        var properties = await GetChapterAdminRestrictedContent(
            request,
            x => x.ChapterPropertyRepository.GetByChapterId(chapter.Id));

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

        return properties.OrderBy(x => x.DisplayOrder).ToArray();
    }

    public async Task<ServiceResult> UpdateChapterQuestion(
        IMemberChapterAdminServiceRequest request, Guid questionId, ChapterQuestionCreateModel model)
    {
        var chapter = request.Chapter;

        var question = await GetChapterAdminRestrictedContent(
            request,
            x => x.ChapterQuestionRepository.GetById(questionId));

        OdkAssertions.BelongsToChapter(question, chapter.Id);

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
        IMemberChapterAdminServiceRequest request, Guid questionId, int moveBy)
    {
        var chapter = request.Chapter;

        var questions = await GetChapterAdminRestrictedContent(
            request,
            x => x.ChapterQuestionRepository.GetByChapterId(chapter.Id));

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

        return questions.OrderBy(x => x.DisplayOrder).ToArray();
    }

    public async Task UpdateChapterRedirectUrl(IMemberChapterAdminServiceRequest request, string? redirectUrl)
    {
        var (chapter, currentMember) = (request.Chapter, request.CurrentMember);

        AssertMemberIsSiteAdmin(currentMember);

        chapter.RedirectUrl = redirectUrl;

        _unitOfWork.ChapterRepository.Update(chapter);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<ServiceResult> UpdateChapterSubscription(
        IMemberChapterAdminServiceRequest request,
        Guid id,
        ChapterSubscriptionCreateModel model)
    {
        var chapter = request.Chapter;

        var (sitePaymentSettings, subscriptions) = await GetChapterAdminRestrictedContent(
            request,
            x => x.SitePaymentSettingsRepository.GetAll(),
            x => x.ChapterSubscriptionRepository.GetByChapterId(chapter.Id, includeDisabled: true));

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

    public async Task<ServiceResult> UpdateChapterTexts(
        IMemberChapterAdminServiceRequest request,
        ChapterTextsUpdateModel model)
    {
        var chapter = request.Chapter;

        var texts = await GetChapterAdminRestrictedContent(
            request,
            x => x.ChapterTextsRepository.GetByChapterId(chapter.Id));

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
        texts.ShortDescription = model.ShortDescription != null
            ? _htmlSanitizer.Sanitize(model.ShortDescription, DefaultHtmlSantizerOptions)
            : null;
        texts.WelcomeText = _htmlSanitizer.Sanitize(model.WelcomeText, DefaultHtmlSantizerOptions);

        if (texts.ChapterId == default)
        {
            texts.ChapterId = chapter.Id;
            _unitOfWork.ChapterTextsRepository.Add(texts);
        }
        else
        {
            _unitOfWork.ChapterTextsRepository.Update(texts);
        }

        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> UpdateChapterTheme(
        IMemberChapterAdminServiceRequest request, ChapterThemeUpdateModel model)
    {
        var chapter = request.Chapter;

        await AssertMemberIsChapterAdmin(request);

        chapter.ThemeBackground = model.Background;
        chapter.ThemeColor = model.Color;

        _unitOfWork.ChapterRepository.Update(chapter);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> UpdateChapterTopics(
        IMemberChapterAdminServiceRequest request,
        IReadOnlyCollection<Guid> topicIds)
    {
        var chapter = request.Chapter;

        var chapterTopics = await GetChapterAdminRestrictedContent(
            request,
            x => x.ChapterTopicRepository.GetByChapterId(chapter.Id));

        var changes = _unitOfWork.ChapterTopicRepository.Merge(chapterTopics, chapter.Id, topicIds);
        if (changes > 0)
        {
            await _unitOfWork.SaveChangesAsync();
        }

        return ServiceResult.Successful();
    }

    private ServiceResult UpdateChapterImage(ChapterImage image, byte[] imageData)
    {
        if (!_imageService.IsImage(imageData))
        {
            return ServiceResult.Failure(ErrorMessagesResource.InvalidImage);
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
            return ServiceResult.Failure(ErrorMessagesResource.RequiredFieldsMissing);
        }

        return ServiceResult.Successful();
    }

    private ServiceResult ValidateChapterMembershipSettings(ChapterMembershipSettings settings)
    {
        if (settings.MembershipDisabledAfterDaysExpired < 0 ||
            settings.TrialPeriodMonths < 0)
        {
            return ServiceResult.Failure(ErrorMessagesResource.RequiredFieldsMissing);
        }

        return ServiceResult.Successful();
    }

    private ServiceResult ValidateChapterProperty(
        ChapterProperty property,
        IReadOnlyCollection<ChapterProperty> existing)
    {
        if (string.IsNullOrEmpty(property.Name) ||
            string.IsNullOrEmpty(property.Label) ||
            !Enum.IsDefined(property.DataType) || property.DataType == DataType.None)
        {
            return ServiceResult.Failure(ErrorMessagesResource.RequiredFieldsMissing);
        }

        if (existing.Any(x => x.Name.Equals(property.Name, StringComparison.OrdinalIgnoreCase) && x.Id != property.Id))
        {
            return ServiceResult.Failure("Name already exists");
        }

        return ServiceResult.Successful();
    }

    private ServiceResult ValidateChapterSubscription(
        ChapterSubscription subscription,
        IReadOnlyCollection<ChapterSubscription> subscriptions)
    {
        if (!Enum.IsDefined(subscription.Type) || subscription.Type == SubscriptionType.None)
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