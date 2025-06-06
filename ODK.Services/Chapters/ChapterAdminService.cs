﻿using ODK.Core;
using ODK.Core.Chapters;
using ODK.Core.Countries;
using ODK.Core.DataTypes;
using ODK.Core.Extensions;
using ODK.Core.Features;
using ODK.Core.Members;
using ODK.Core.Notifications;
using ODK.Core.Platforms;
using ODK.Core.Subscriptions;
using ODK.Core.Web;
using ODK.Data.Core;
using ODK.Data.Core.Deferred;
using ODK.Services.Caching;
using ODK.Services.Chapters.Models;
using ODK.Services.Chapters.ViewModels;
using ODK.Services.Imaging;
using ODK.Services.Members;
using ODK.Services.Notifications;
using ODK.Services.Payments;
using ODK.Services.SocialMedia;
using ODK.Services.Topics;

namespace ODK.Services.Chapters;

public class ChapterAdminService : OdkAdminServiceBase, IChapterAdminService
{    
    private readonly ICacheService _cacheService;
    private readonly IHtmlSanitizer _htmlSanitizer;
    private readonly IImageService _imageService;
    private readonly IInstagramService _instagramService;
    private readonly IMemberEmailService _memberEmailService;
    private readonly INotificationService _notificationService;
    private readonly IPaymentService _paymentService;
    private readonly IPlatformProvider _platformProvider;
    private readonly ChapterAdminServiceSettings _settings;
    private readonly ITopicService _topicService;
    private readonly IUnitOfWork _unitOfWork;

    public ChapterAdminService(
        IUnitOfWork unitOfWork, 
        ICacheService cacheService,
        IHtmlSanitizer htmlSanitizer,
        IInstagramService instagramService,
        IPlatformProvider platformProvider,
        INotificationService notificationService,
        IImageService imageService,
        IMemberEmailService memberEmailService,
        ITopicService topicService,
        IPaymentService paymentService,
        ChapterAdminServiceSettings settings)
        : base(unitOfWork)
    {
        _cacheService = cacheService;
        _htmlSanitizer = htmlSanitizer;
        _imageService = imageService;
        _instagramService = instagramService;
        _memberEmailService = memberEmailService;
        _notificationService = notificationService;
        _paymentService = paymentService;
        _platformProvider = platformProvider;
        _settings = settings;
        _topicService = topicService;
        _unitOfWork = unitOfWork;
    }
    
    public async Task<ServiceResult> AddChapterAdminMember(AdminServiceRequest request, Guid memberId)
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

    public async Task<ServiceResult> ApproveChapter(AdminServiceRequest request)
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

        await _memberEmailService.SendGroupApprovedEmail(chapter, owner);

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult<Chapter?>> CreateChapter(Guid currentMemberId, ChapterCreateModel model)
    {
        var now = DateTime.UtcNow;
        var platform = _platformProvider.GetPlatform();

        var (
            memberSubscription, 
            memberPaymentSettings, 
            existing, 
            countries, 
            siteEmailSettings
        ) = await _unitOfWork.RunAsync(
            x => x.MemberSiteSubscriptionRepository.GetByMemberId(currentMemberId, platform),
            x => x.MemberPaymentSettingsRepository.GetByMemberId(currentMemberId),
            x => x.ChapterRepository.GetAll(),
            x => x.CountryRepository.GetAll(),
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

        var image = new ChapterImage();

        var result = UpdateChapterImage(image, model.ImageData);
        if (!result.Success)
        {
            return ServiceResult<Chapter?>.Failure(result.Message ?? "");
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
            CreatedUtc = now
        });

        _unitOfWork.ChapterTopicRepository.AddMany(model.TopicIds.Select(x => new ChapterTopic
        {
            ChapterId = chapter.Id,
            TopicId = x
        }));

        if (memberPaymentSettings != null)
        {
            _unitOfWork.ChapterPaymentSettingsRepository.Add(new ChapterPaymentSettings
            {
                ApiPublicKey = memberPaymentSettings.ApiPublicKey,
                ApiSecretKey = memberPaymentSettings.ApiSecretKey,
                ChapterId = chapter.Id,
                CurrencyId = memberPaymentSettings.CurrencyId
            });
        }

        image.ChapterId = chapter.Id;
        _unitOfWork.ChapterImageRepository.Add(image);

        await _unitOfWork.SaveChangesAsync();

        await _topicService.AddNewChapterTopics(currentMemberId, chapter.Id, model.NewTopics);

        await _memberEmailService.SendNewGroupEmail(chapter, texts, siteEmailSettings);

        return ServiceResult<Chapter?>.Successful(chapter);
    }

    public async Task<ServiceResult> CreateChapterProperty(AdminServiceRequest request, 
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

    public async Task<ServiceResult> CreateChapterQuestion(AdminServiceRequest request,
        CreateChapterQuestion model)
    {
        var chapterId = request.ChapterId;

        var existing = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterQuestionRepository.GetByChapterId(chapterId));

        var displayOrder = existing.Count  > 0 ? existing.Max(x => x.DisplayOrder) + 1 : 1;
        
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

    public async Task<ServiceResult> CreateChapterSubscription(AdminServiceRequest request, 
        CreateChapterSubscription model)
    {
        var chapterId = request.ChapterId;
        
        var (
            chapter,
            ownerSubscription, 
            existing, 
            chapterPaymentSettings,
            sitePaymentSettings
        ) = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterRepository.GetById(chapterId),
            x => x.MemberSiteSubscriptionRepository.GetByChapterId(request.ChapterId),
            x => x.ChapterSubscriptionRepository.GetByChapterId(chapterId, includeDisabled: true),
            x => x.ChapterPaymentSettingsRepository.GetByChapterId(request.ChapterId),
            x => x.SitePaymentSettingsRepository.GetActive());

        if (ownerSubscription?.HasFeature(SiteFeatureType.MemberSubscriptions) != true)
        {
            return ServiceResult.Failure("Not permitted");
        }

        var subscription = new ChapterSubscription
        {
            Amount = model.Amount,
            ChapterId = chapterId,
            Description = _htmlSanitizer.Sanitize(model.Description, DefaultHtmlSantizerOptions),
            Months = model.Months,
            Name = model.Name,
            Recurring = model.Recurring,
            Title = model.Title,
            Type = model.Type,
            SitePaymentSettingId = chapterPaymentSettings?.UseSitePaymentProvider == true
                ? sitePaymentSettings.Id
                : null
        };        

        var validationResult = ValidateChapterSubscription(subscription, existing);
        if (!validationResult.Success)
        {
            return validationResult;
        }

        if (chapterPaymentSettings?.UseSitePaymentProvider == true)
        {      
            var platform = _platformProvider.GetPlatform();
            var productName = chapter.GetFullName(platform);

            var productId = await _paymentService.GetProductId(sitePaymentSettings, productName);
            if (string.IsNullOrEmpty(productId))
            {
                productId = await _paymentService.CreateProduct(sitePaymentSettings, productName);
            }

            if (productId == null)
            {
                throw new Exception("Error creating product");
            }

            subscription.ExternalProductId = productId;

            var externalId = await _paymentService.CreateSubscriptionPlan(
                sitePaymentSettings,
                new ExternalSubscriptionPlan
                {
                    Amount = (decimal)subscription.Amount,
                    CurrencyCode = chapterPaymentSettings.Currency.Code,
                    ExternalId = "",
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

            await _paymentService.ActivateSubscriptionPlan(sitePaymentSettings, externalId);
        }

        _unitOfWork.ChapterSubscriptionRepository.Add(subscription);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }
    
    public async Task<ServiceResult> DeleteChapter(AdminServiceRequest request)
    {
        var chapter = await GetSuperAdminRestrictedContent(request,
            x => x.ChapterRepository.GetById(request.ChapterId));

        _unitOfWork.ChapterRepository.Delete(chapter);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> DeleteChapterAdminMember(AdminServiceRequest request,
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

    public async Task<ServiceResult> DeleteChapterContactMessage(AdminServiceRequest request, Guid id)
    {
        var contactMessage = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterContactMessageRepository.GetById(id));

        OdkAssertions.BelongsToChapter(contactMessage, request.ChapterId);

        _unitOfWork.ChapterContactMessageRepository.Delete(contactMessage);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    public async Task DeleteChapterProperty(AdminServiceRequest request, Guid id)
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

    public async Task DeleteChapterQuestion(AdminServiceRequest request, Guid id)
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

    public async Task<ServiceResult> DeleteChapterSubscription(AdminServiceRequest request, Guid id)
    {
        var subscription = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterSubscriptionRepository.GetById(id));

        _unitOfWork.ChapterSubscriptionRepository.Delete(subscription);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    public async Task<Chapter> GetChapter(AdminServiceRequest request)
    {
        return await GetChapterAdminRestrictedContent(request,
            x => x.ChapterRepository.GetById(request.ChapterId));
    }

    public async Task<IReadOnlyCollection<ChapterAdminMember>> GetChapterAdminMembers(AdminServiceRequest request)
    {
        var (chapterId, currentMemberId) = (request.ChapterId, request.CurrentMemberId);

        var (chapterAdminMembers, currentMember) = await _unitOfWork.RunAsync(
            x => x.ChapterAdminMemberRepository.GetByChapterId(chapterId),
            x => x.MemberRepository.GetById(currentMemberId));
        
        AssertMemberIsChapterAdmin(currentMember, chapterId, chapterAdminMembers);

        return chapterAdminMembers;
    }

    public async Task<ChapterAdminPageViewModel> GetChapterAdminPageViewModel(AdminServiceRequest request)
    {
        var platform = _platformProvider.GetPlatform();

        var chapter = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterRepository.GetById(request.ChapterId));

        return new ChapterAdminPageViewModel
        {
            Chapter = chapter,
            Platform = platform
        };
    }

    public async Task<ChapterConversationsAdminPageViewModel> GetChapterConversationsViewModel(AdminServiceRequest request, bool readByChapter)
    {
        var platform = _platformProvider.GetPlatform();

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
        AdminServiceRequest request, Guid id)
    {
        var platform = _platformProvider.GetPlatform();

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

    public async Task<ChapterDeleteAdminPageViewModel> GetChapterDeleteViewModel(AdminServiceRequest request)
    {
        var platform = _platformProvider.GetPlatform();

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

    public async Task<ChapterImageAdminPageViewModel> GetChapterImageViewModel(AdminServiceRequest request)
    {
        var platform = _platformProvider.GetPlatform();

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

    public async Task<ChapterLinksAdminPageViewModel> GetChapterLinksViewModel(AdminServiceRequest request)
    {
        var platform = _platformProvider.GetPlatform();

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

    public async Task<ChapterLocation?> GetChapterLocation(AdminServiceRequest request)
    {
        var chapter = await GetSuperAdminRestrictedContent(request,
            x => x.ChapterRepository.GetById(request.ChapterId));

        var location = await _unitOfWork.ChapterLocationRepository.GetByChapterId(request.ChapterId);
        return location;
    }

    public async Task<ChapterMessagesAdminPageViewModel> GetChapterMessagesViewModel(
        AdminServiceRequest request, bool spam)
    {
        var platform = _platformProvider.GetPlatform();

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

    public async Task<ChapterMessageAdminPageViewModel> GetChapterMessageViewModel(AdminServiceRequest request, Guid id)
    {
        var platform = _platformProvider.GetPlatform();

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

    public async Task<ChapterPaymentSettings?> GetChapterPaymentSettings(AdminServiceRequest request)
    {
        return await GetChapterAdminRestrictedContent(request,
            x => x.ChapterPaymentSettingsRepository.GetByChapterId(request.ChapterId));
    }

    public async Task<ChapterPaymentSettingsAdminPageViewModel> GetChapterPaymentSettingsViewModel(AdminServiceRequest request)
    {
        var platform = _platformProvider.GetPlatform();

        var (chapter, paymentSettings, currencies, ownerSubscription) = await _unitOfWork.RunAsync(
            x => x.ChapterRepository.GetById(request.ChapterId),
            x => x.ChapterPaymentSettingsRepository.GetByChapterId(request.ChapterId),
            x => x.CurrencyRepository.GetAll(),
            x => x.MemberSiteSubscriptionRepository.GetByChapterId(request.ChapterId));

        return new ChapterPaymentSettingsAdminPageViewModel
        {
            Chapter = chapter,
            CurrencyOptions = currencies,
            OwnerSubscription = ownerSubscription,
            PaymentSettings = paymentSettings,
            Platform = platform
        };
    }

    public async Task<ChapterPrivacyAdminPageViewModel> GetChapterPrivacyViewModel(AdminServiceRequest request)
    {
        var platform = _platformProvider.GetPlatform();

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

    public async Task<IReadOnlyCollection<ChapterProperty>> GetChapterProperties(AdminServiceRequest request)
    {
        return await GetChapterAdminRestrictedContent(request,
            x => x.ChapterPropertyRepository.GetByChapterId(request.ChapterId));
    }

    public async Task<ChapterPropertiesAdminPageViewModel> GetChapterPropertiesViewModel(AdminServiceRequest request)
    {
        var platform = _platformProvider.GetPlatform();

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
        AdminServiceRequest request, 
        Guid propertyId)
    {
        var platform = _platformProvider.GetPlatform();

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

    public async Task<ChapterQuestion> GetChapterQuestion(AdminServiceRequest request, Guid questionId)
    {
        var question = await GetChapterAdminRestrictedContent(request, 
            x => x.ChapterQuestionRepository.GetById(questionId));
        OdkAssertions.BelongsToChapter(question, request.ChapterId);
        return question;
    }

    public async Task<ChapterQuestionsAdminPageViewModel> GetChapterQuestionsViewModel(AdminServiceRequest request)
    {
        var platform = _platformProvider.GetPlatform();

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

    public async Task<ChapterQuestionAdminPageViewModel> GetChapterQuestionViewModel(AdminServiceRequest request, Guid questionId)
    {
        var platform = _platformProvider.GetPlatform();

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

    public async Task<IReadOnlyCollection<ChapterQuestion>> GetChapterQuestions(AdminServiceRequest request)
    {
        return await GetChapterAdminRestrictedContent(request,
            x => x.ChapterQuestionRepository.GetByChapterId(request.ChapterId));
    }

    public async Task<ChapterTextsAdminPageViewModel> GetChapterTextsViewModel(AdminServiceRequest request)
    {
        var platform = _platformProvider.GetPlatform();

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

    public async Task<ChapterTopicsAdminPageViewModel> GetChapterTopicsViewModel(AdminServiceRequest request)
    {
        var platform = _platformProvider.GetPlatform();

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

    public async Task<MembershipSettingsAdminPageViewModel> GetMembershipSettingsViewModel(AdminServiceRequest request)
    {
        var platform = _platformProvider.GetPlatform();

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

    public async Task<SuperAdminChaptersViewModel> GetSuperAdminChaptersViewModel(Guid currentMemberId)
    {
        var chapters = await GetSuperAdminRestrictedContent(currentMemberId,
            x => x.ChapterRepository.GetAll());

        return new SuperAdminChaptersViewModel
        {
            PendingApproval = chapters
                .Where(x => !x.Approved())
                .OrderBy(x => x.CreatedUtc)
                .ToArray()
        };
    }

    public async Task<ServiceResult> PublishChapter(AdminServiceRequest request)
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

    public async Task<ServiceResult> ReplyToConversation(AdminServiceRequest request, Guid conversationId, string message)
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
        
        await _memberEmailService.SendChapterConversationEmail(chapter, conversation, conversationMessage, [member], isReply: true);

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> ReplyToMessage(AdminServiceRequest request, Guid messageId, string message)
    {
        var (chapter, adminMembers, originalMessage) = await _unitOfWork.RunAsync(
            x => x.ChapterRepository.GetById(request.ChapterId),
            x => x.ChapterAdminMemberRepository.GetByChapterId(request.ChapterId),
            x => x.ChapterContactMessageRepository.GetById(messageId));

        var adminMember = adminMembers.FirstOrDefault(x => x.MemberId == request.CurrentMemberId);

        OdkAssertions.Exists(adminMember);
        OdkAssertions.BelongsToChapter(originalMessage, request.ChapterId);

        var sendResult = await _memberEmailService.SendChapterMessageReply(chapter, originalMessage, message);
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

    public async Task<ServiceResult> SetMessageAsReplied(AdminServiceRequest request, Guid messageId)
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

    public async Task SetOwner(AdminServiceRequest request, Guid memberId)
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

    public async Task<ServiceResult> StartConversation(AdminServiceRequest request, Guid memberId, string subject, string message)
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

        await _memberEmailService.SendChapterConversationEmail(chapter, conversation, conversationMessage, [member], 
            isReply: false);

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> UpdateChapterAdminMember(AdminServiceRequest request, Guid memberId,
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

    public async Task<ServiceResult> UpdateChapterImage(AdminServiceRequest request, UpdateChapterImage model)
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

    public async Task UpdateChapterLinks(AdminServiceRequest request, UpdateChapterLinks model)
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

    public async Task<ServiceResult> UpdateChapterCurrency(AdminServiceRequest request, Guid currencyId)
    {
        var chapter = await _unitOfWork.ChapterRepository.GetById(request.ChapterId).Run();

        var (chapterPaymentSettings, currencies, ownerPaymentSettings) = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterPaymentSettingsRepository.GetByChapterId(request.ChapterId),
            x => x.CurrencyRepository.GetAll(),
            x => chapter.OwnerId != null 
                ? x.MemberPaymentSettingsRepository.GetByMemberId(chapter.OwnerId.Value)
                : new DefaultDeferredQuerySingleOrDefault<MemberPaymentSettings>());

        if (!currencies.Any(x => x.Id == currencyId))
        {
            return ServiceResult.Failure("Currency not found");
        }

        chapterPaymentSettings ??= new ChapterPaymentSettings();        

        chapterPaymentSettings.CurrencyId = currencyId;

        if (chapterPaymentSettings.ChapterId == default)
        {
            chapterPaymentSettings.ChapterId = request.ChapterId;
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

    public async Task<ServiceResult> UpdateChapterDescription(AdminServiceRequest request, string description)
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

    public async Task<ServiceResult> UpdateChapterLocation(AdminServiceRequest request,
        LatLong? location, string? name)
    {
        var chapter = await GetSuperAdminRestrictedContent(request,
            x => x.ChapterRepository.GetById(request.ChapterId));

        var chapterLocation = await _unitOfWork.ChapterLocationRepository.GetByChapterId(request.ChapterId);

        if (location != null && !string.IsNullOrEmpty(name))
        {
            chapterLocation ??= new ChapterLocation();            

            chapterLocation.LatLong = location.Value;
            chapterLocation.Name = name;
        }        
        else
        {
            if (chapterLocation == null)
            {
                return ServiceResult.Successful();
            }

            _unitOfWork.ChapterLocationRepository.Delete(chapterLocation);
            await _unitOfWork.SaveChangesAsync();
            return ServiceResult.Successful();
        }

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

    public async Task<ServiceResult> UpdateChapterMembershipSettings(AdminServiceRequest request, 
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

    public async Task<ServiceResult> UpdateChapterPaymentSettings(AdminServiceRequest request,
        UpdateChapterPaymentSettings model)
    {
        var chapterId = request.ChapterId;

        if (model.CurrencyId == null)
        {
            return ServiceResult.Failure("Currency required");
        }

        var (settings, currency) = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterPaymentSettingsRepository.GetByChapterId(chapterId),
            x => x.CurrencyRepository.GetById(model.CurrencyId.Value));

        settings ??= new();

        settings.ApiPublicKey = model.ApiPublicKey;
        settings.ApiSecretKey = model.ApiSecretKey;
        settings.CurrencyId = model.CurrencyId.Value;
        settings.Provider = model.Provider;
        settings.EmailAddress = model.EmailAddress;
        settings.UseSitePaymentProvider = model.UseSitePaymentProvider;

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
        AdminServiceRequest request,
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

    public async Task<ServiceResult> UpdateChapterProperty(AdminServiceRequest request, 
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
        AdminServiceRequest request, Guid propertyId, int moveBy)
    {
        var properties = await GetChapterProperties(request);
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

    public async Task<ServiceResult> UpdateChapterQuestion(AdminServiceRequest request, Guid questionId, CreateChapterQuestion model)
    {
        var question = await GetChapterQuestion(request, questionId);

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
        AdminServiceRequest request, Guid questionId, int moveBy)
    {
        var questions = await GetChapterQuestions(request);
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

    public async Task UpdateChapterRedirectUrl(AdminServiceRequest request, string? redirectUrl)
    {
        var chapter = await GetSuperAdminRestrictedContent(request,
            x => x.ChapterRepository.GetById(request.ChapterId));

        chapter.RedirectUrl = redirectUrl;

        _unitOfWork.ChapterRepository.Update(chapter);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<ServiceResult> UpdateChapterSiteSubscription(AdminServiceRequest request,
        Guid siteSubscriptionId, SiteSubscriptionFrequency frequency)
    {
        var (chapter, chapterPaymentSettings, siteSubscription) = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterRepository.GetById(request.ChapterId),
            x => x.ChapterPaymentSettingsRepository.GetByChapterId(request.ChapterId),
            x => x.SiteSubscriptionPriceRepository.GetBySiteSubscriptionId(siteSubscriptionId));

        if (chapter.OwnerId == null)
        {
            return ServiceResult.Failure("Chapter owner not set");
        }

        if (chapterPaymentSettings == null || !chapterPaymentSettings.HasApiKey)
        {
            return ServiceResult.Failure("Chapter payment settings not set");
        }

        throw new NotImplementedException();
    }

    public async Task<ServiceResult> UpdateChapterSubscription(AdminServiceRequest request, 
        Guid id, CreateChapterSubscription model)
    {
        var subscriptions = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterSubscriptionRepository.GetByChapterId(request.ChapterId, includeDisabled: true));

        var subscription = subscriptions.FirstOrDefault(x => x.Id == id);
        OdkAssertions.Exists(subscription);

        // subscription.Amount = model.Amount;
        subscription.Description = model.Description;
        subscription.Disabled = model.Disabled;
        // subscription.Months = model.Months;
        subscription.Name = model.Name;
        subscription.Title = model.Title;
        subscription.Type = model.Type;

        var validationResult = ValidateChapterSubscription(subscription, subscriptions);
        if (!validationResult.Success)
        {
            return validationResult;
        }

        _unitOfWork.ChapterSubscriptionRepository.Update(subscription);
        await _unitOfWork.SaveChangesAsync();        

        return ServiceResult.Successful();
    }
    
    public async Task<ServiceResult> UpdateChapterTexts(AdminServiceRequest request,
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
    
    public async Task<ServiceResult> UpdateChapterTimeZone(AdminServiceRequest request, string? timeZoneId)
    {
        var chapter = await GetSuperAdminRestrictedContent(request,
            x => x.ChapterRepository.GetById(request.ChapterId));

        TimeZoneInfo? timeZone = null;
        if (!string.IsNullOrEmpty(timeZoneId) && !TimeZoneInfo.TryFindSystemTimeZoneById(timeZoneId, out timeZone))
        {
            return ServiceResult.Failure("Invalid time zone id");
        }

        chapter.TimeZone = timeZone;

        _unitOfWork.ChapterRepository.Update(chapter);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> UpdateChapterTopics(AdminServiceRequest request,
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
            settings.TrialPeriodMonths <= 0)
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
