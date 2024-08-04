using ODK.Core;
using ODK.Core.Chapters;
using ODK.Core.DataTypes;
using ODK.Core.Members;
using ODK.Data.Core;
using ODK.Services.Caching;

namespace ODK.Services.Chapters;

public class ChapterAdminService : OdkAdminServiceBase, IChapterAdminService
{
    private readonly ICacheService _cacheService;
    private readonly IChapterService _chapterService;
    private readonly IUnitOfWork _unitOfWork;

    public ChapterAdminService(IUnitOfWork unitOfWork, ICacheService cacheService,
        IChapterService chapterService)
        : base(unitOfWork)
    {
        _cacheService = cacheService;
        _chapterService = chapterService;
        _unitOfWork = unitOfWork;
    }
    
    public async Task<ServiceResult> AddChapterAdminMember(AdminServiceRequest request, Guid memberId)
    {
        var (chapterId, currentMemberId) = (request.ChapterId, request.CurrentMemberId);

        var (chapterAdminMembers, currentMember, member) = await _unitOfWork.RunAsync(
            x => x.ChapterAdminMemberRepository.GetByChapterId(chapterId),
            x => x.MemberRepository.GetById(currentMemberId),
            x => x.MemberRepository.GetById(memberId));

        AssertMemberIsChapterAdmin(currentMember, chapterId, chapterAdminMembers);

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

    public async Task<ServiceResult> CreateChapterProperty(AdminServiceRequest request, 
        CreateChapterProperty model)
    {
        var chapterId = request.ChapterId;

        var existing = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterPropertyRepository.GetByChapterId(chapterId, all: true));

        var displayOrder = existing.Count > 0 ? existing.Max(x => x.DisplayOrder) + 1 : 1;

        var property = new ChapterProperty
        {
            ChapterId = chapterId,
            DataType = model.DataType,
            DisplayOrder = displayOrder,
            HelpText = model.HelpText,
            Hidden = model.Hidden,
            Label = model.Label,
            Name = model.Name.ToLowerInvariant(),
            Required = model.Required,
            Subtitle = model.Subtitle
        };

        var validationResult = ValidateChapterProperty(property, existing);
        if (!validationResult.Success)
        {
            return validationResult;
        }

        _unitOfWork.ChapterPropertyRepository.Add(property);
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
            Answer = model.Answer,
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

        var existing = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterSubscriptionRepository.GetByChapterId(chapterId));

        var subscription = new ChapterSubscription
        {
            Amount = model.Amount,
            ChapterId = chapterId,
            Description = model.Description,
            Months = model.Months,
            Name = model.Name,
            Title = model.Title,
            Type = model.Type
        };

        var validationResult = ValidateChapterSubscription(subscription, existing);
        if (!validationResult.Success)
        {
            return validationResult;
        }

        _unitOfWork.ChapterSubscriptionRepository.Add(subscription);
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

    public async Task<ServiceResult> DeleteChapterContactRequest(AdminServiceRequest request, Guid id)
    {
        var contactRequest = await GetChapterAdminRestrictedContent(request,
            x => x.ContactRequestRepository.GetById(id));

        OdkAssertions.BelongsToChapter(contactRequest, request.ChapterId);

        _unitOfWork.ContactRequestRepository.Delete(contactRequest);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    public async Task DeleteChapterProperty(AdminServiceRequest request, Guid id)
    {        
        var properties = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterPropertyRepository.GetByChapterId(request.ChapterId, true));

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

    public async Task<ChapterAdminMember> GetChapterAdminMember(AdminServiceRequest request, Guid memberId)
    {
        var (chapterId, currentMemberId) = (request.ChapterId, request.CurrentMemberId);

        var (chapterAdminMembers, currentMember, member) = await _unitOfWork.RunAsync(
            x => x.ChapterAdminMemberRepository.GetByChapterId(chapterId),
            x => x.MemberRepository.GetById(currentMemberId),
            x => x.MemberRepository.GetById(memberId));

        AssertMemberIsChapterAdmin(currentMember, chapterId, chapterAdminMembers);

        var adminMember = chapterAdminMembers.FirstOrDefault(x => x.MemberId == memberId);
        return OdkAssertions.Exists(adminMember);
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

    public async Task<IReadOnlyCollection<ContactRequest>> GetChapterContactRequests(
        AdminServiceRequest request)
    {
        return await GetChapterAdminRestrictedContent(request,
            x => x.ContactRequestRepository.GetByChapterId(request.ChapterId));
    }

    public async Task<ChapterEventSettings?> GetChapterEventSettings(AdminServiceRequest request)
    {
        return await GetChapterAdminRestrictedContent(request,
            x => x.ChapterEventSettingsRepository.GetByChapterId(request.ChapterId));
    }

    public async Task<ChapterLinks?> GetChapterLinks(AdminServiceRequest request)
    {
        return await GetChapterAdminRestrictedContent(request,
            x => x.ChapterLinksRepository.GetByChapterId(request.ChapterId));
    }

    public async Task<ChapterMembershipSettings?> GetChapterMembershipSettings(AdminServiceRequest request)
    {
        return await GetChapterAdminRestrictedContent(request,
            x => x.ChapterMembershipSettingsRepository.GetByChapterId(request.ChapterId));
    }

    public async Task<ChapterMemberSubscriptionsDto> GetChapterMemberSubscriptionsDto(
        AdminServiceRequest request)
    {
        var (chapterId, currentMemberId) = (request.ChapterId, request.CurrentMemberId);

        var chapter = await _unitOfWork.ChapterRepository.GetById(chapterId).RunAsync();

        var (chapterAdminMembers, currentMember, memberSubscription, chapterSubscriptions, country, paymentSettings, membershipSettings) = await _unitOfWork.RunAsync(
            x => x.ChapterAdminMemberRepository.GetByMemberId(currentMemberId),
            x => x.MemberRepository.GetById(currentMemberId),
            x => x.MemberSubscriptionRepository.GetByMemberId(currentMemberId, chapterId),
            x => x.ChapterSubscriptionRepository.GetByChapterId(chapterId),
            x => x.CountryRepository.GetById(chapter.CountryId),
            x => x.ChapterPaymentSettingsRepository.GetByChapterId(chapterId),
            x => x.ChapterMembershipSettingsRepository.GetByChapterId(chapterId));

        AssertMemberIsChapterAdmin(currentMember, chapterId, chapterAdminMembers);

        return new ChapterMemberSubscriptionsDto
        {
            ChapterSubscriptions = chapterSubscriptions,
            Country = country,
            MembershipSettings = membershipSettings ?? new(),
            MemberSubscription = memberSubscription,
            PaymentSettings = paymentSettings
        };
    }

    public async Task<ChapterPaymentSettings?> GetChapterPaymentSettings(AdminServiceRequest request)
    {
        return await GetChapterAdminRestrictedContent(request,
            x => x.ChapterPaymentSettingsRepository.GetByChapterId(request.ChapterId));
    }

    public async Task<IReadOnlyCollection<ChapterProperty>> GetChapterProperties(AdminServiceRequest request)
    {
        return await GetChapterAdminRestrictedContent(request,
            x => x.ChapterPropertyRepository.GetByChapterId(request.ChapterId));
    }

    public async Task<ChapterProperty> GetChapterProperty(AdminServiceRequest request, Guid id)
    {
        var property = await GetChapterAdminRestrictedContent(request, 
            x => x.ChapterPropertyRepository.GetById(id));
        OdkAssertions.BelongsToChapter(property, request.ChapterId);
        return property;
    }

    public async Task<ChapterQuestion> GetChapterQuestion(AdminServiceRequest request, Guid questionId)
    {
        var question = await GetChapterAdminRestrictedContent(request, 
            x => x.ChapterQuestionRepository.GetById(questionId));
        OdkAssertions.BelongsToChapter(question, request.ChapterId);
        return question;
    }

    public async Task<ChapterSubscription> GetChapterSubscription(AdminServiceRequest request, Guid id)
    {
        var subscription = await GetChapterAdminRestrictedContent(request, 
            x => x.ChapterSubscriptionRepository.GetById(id));
        OdkAssertions.BelongsToChapter(subscription, request.ChapterId);
        return subscription;
    }

    public async Task<IReadOnlyCollection<ChapterQuestion>> GetChapterQuestions(AdminServiceRequest request)
    {
        return await GetChapterAdminRestrictedContent(request,
            x => x.ChapterQuestionRepository.GetByChapterId(request.ChapterId));
    }

    public async Task<IReadOnlyCollection<ChapterSubscription>> GetChapterSubscriptions(AdminServiceRequest request)
    {
        return await GetChapterAdminRestrictedContent(request,
            x => x.ChapterSubscriptionRepository.GetByChapterId(request.ChapterId));        
    }

    public async Task<ChapterTexts?> GetChapterTexts(AdminServiceRequest request)
    {
        return await GetChapterAdminRestrictedContent(request,
            x => x.ChapterTextsRepository.GetByChapterId(request.ChapterId));
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

    public async Task UpdateChapterLinks(AdminServiceRequest request, UpdateChapterLinks update)
    {
        var links = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterLinksRepository.GetByChapterId(request.ChapterId));

        if (links == null)
        {
            links = new ChapterLinks();            
        }

        links.FacebookName = update.Facebook;
        links.TwitterName = update.Twitter;
        links.InstagramName = update.Instagram;        
        
        if (links.ChapterId == default)
        {
            links.ChapterId = request.ChapterId;
            _unitOfWork.ChapterLinksRepository.Add(links);
        }
        else
        {
            _unitOfWork.ChapterLinksRepository.Update(links);
        }

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task UpdateChapterEventSettings(AdminServiceRequest request, UpdateChapterEventSettings model)
    {
        var settings = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterEventSettingsRepository.GetByChapterId(request.ChapterId));

        if (settings == null)
        {
            settings = new();
        }

        settings.DefaultDayOfWeek = model.DefaultDayOfWeek;
        settings.DefaultDescription = model.DefaultDescription;
        settings.DefaultScheduledEmailDayOfWeek = model.DefaultScheduledEmailDayOfWeek;
        settings.DefaultScheduledEmailTimeOfDay = model.DefaultScheduledEmailTimeOfDay;
        settings.DisableComments = model.DisableComments;

        if (settings.ChapterId == default)
        {
            settings.ChapterId = request.ChapterId;
            _unitOfWork.ChapterEventSettingsRepository.Add(settings);
        }
        else
        {
            _unitOfWork.ChapterEventSettingsRepository.Update(settings);
        }

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<ServiceResult> UpdateChapterMembershipSettings(AdminServiceRequest request, 
        UpdateChapterMembershipSettings model)
    {
        var settings = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterMembershipSettingsRepository.GetByChapterId(request.ChapterId));
        if (settings == null)
        {
            settings = new ChapterMembershipSettings();
        }

        settings.Enabled = model.Enabled;
        settings.MembershipDisabledAfterDaysExpired = model.MembershipDisabledAfterDaysExpired;
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

        var settings = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterPaymentSettingsRepository.GetByChapterId(chapterId));

        if (settings == null)
        {
            settings = new();
        }

        settings.ApiPublicKey = model.ApiPublicKey;
        settings.ApiSecretKey = model.ApiSecretKey;
        settings.Provider = model.Provider;

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
    
    public async Task<ServiceResult> UpdateChapterProperty(AdminServiceRequest request, 
        Guid propertyId, UpdateChapterProperty model)
    {
        var properties = await GetChapterAdminRestrictedContent(request,
             x => x.ChapterPropertyRepository.GetByChapterId(request.ChapterId));

        var property = properties.FirstOrDefault(x => x.Id == propertyId);
        OdkAssertions.Exists(property);
        OdkAssertions.BelongsToChapter(property, request.ChapterId);

        property.HelpText = model.HelpText;
        property.Hidden = model.Hidden;
        property.Label = model.Label;
        property.Name = model.Name.ToLowerInvariant();
        property.Required = model.Required;
        property.Subtitle = model.Subtitle;

        var validationResult = ValidateChapterProperty(property, properties);
        if (!validationResult.Success)
        {
            return validationResult;
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

    public async Task<ServiceResult> UpdateChapterQuestion(AdminServiceRequest request, Guid questionId, CreateChapterQuestion update)
    {
        var question = await GetChapterQuestion(request, questionId);

        question.Answer = update.Answer;
        question.Name = update.Name;

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

    public async Task<ServiceResult> UpdateChapterSubscription(AdminServiceRequest request, 
        Guid id, CreateChapterSubscription model)
    {
        var subscriptions = await GetChapterAdminRestrictedContent(request,
            x => x.ChapterSubscriptionRepository.GetByChapterId(request.ChapterId));
        var subscription = subscriptions.FirstOrDefault(x => x.Id == id);
        OdkAssertions.Exists(subscription);

        subscription.Amount = model.Amount;
        subscription.Description = model.Description;
        subscription.Months = model.Months;
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

        texts.RegisterText = model.RegisterText;
        texts.WelcomeText = model.WelcomeText;

        _unitOfWork.ChapterTextsRepository.Update(texts);
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

        if (subscriptions.Any(x => x.Id != subscription.Id && x.Name.Equals(subscription.Name)))
        {
            return ServiceResult.Failure("A subscription with that name already exists");
        }

        return ServiceResult.Successful();
    }
}
