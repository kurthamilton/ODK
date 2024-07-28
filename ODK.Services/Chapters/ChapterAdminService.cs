using ODK.Core.Chapters;
using ODK.Core.DataTypes;
using ODK.Core.Exceptions;
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
    
    public async Task<ServiceResult> AddChapterAdminMember(Guid currentMemberId, Guid chapterId, Guid memberId)
    {
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

    public async Task<ServiceResult> CreateChapterProperty(Guid currentMemberId, Guid chapterId, 
        CreateChapterProperty model)
    {
        var existing = await GetChapterAdminRestrictedContent(currentMemberId, chapterId,
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

    public async Task<ServiceResult> CreateChapterQuestion(Guid currentMemberId, Guid chapterId,
        CreateChapterQuestion model)
    {
        var existing = await GetChapterAdminRestrictedContent(currentMemberId, chapterId,
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

    public async Task<ServiceResult> CreateChapterSubscription(Guid currentMemberId, Guid chapterId, 
        CreateChapterSubscription model)
    {
        var existing = await GetChapterAdminRestrictedContent(currentMemberId, chapterId,
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
    
    public async Task<ServiceResult> DeleteChapterAdminMember(Guid currentMemberId, Guid chapterId,
        Guid memberId)
    {
        var (chapterAdminMembers, currentMember, member) = await _unitOfWork.RunAsync(
            x => x.ChapterAdminMemberRepository.GetByChapterId(chapterId),
            x => x.MemberRepository.GetById(currentMemberId),
            x => x.MemberRepository.GetById(memberId));

        var adminMember = chapterAdminMembers.FirstOrDefault(x => x.MemberId == memberId);
        if (adminMember == null)
        {
            return ServiceResult.Failure("Admin member not found");
        }

        if (member.SuperAdmin)
        {
            return ServiceResult.Failure("Cannot delete a super admin");
        }

        _unitOfWork.ChapterAdminMemberRepository.Delete(adminMember);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> DeleteChapterContactRequest(Guid currentMemberId, Guid id)
    {
        var contactRequest = await _unitOfWork.ContactRequestRepository.GetByIdOrDefault(id).RunAsync();
        if (contactRequest == null)
        {
            return ServiceResult.Failure("Contact request not found");
        }

        await AssertMemberIsChapterAdmin(currentMemberId, contactRequest.ChapterId);

        _unitOfWork.ContactRequestRepository.Delete(contactRequest);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    public async Task DeleteChapterProperty(Guid currentMemberId, Guid id)
    {
        var property = await GetChapterProperty(currentMemberId, id);        

        var properties = await _unitOfWork.ChapterPropertyRepository.GetByChapterId(property.ChapterId, true).RunAsync();
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

    public async Task DeleteChapterQuestion(Guid currentMemberId, Guid id)
    {
        var question = await GetChapterQuestion(currentMemberId, id);        

        var questions = await _unitOfWork.ChapterQuestionRepository.GetByChapterId(question.ChapterId).RunAsync();
        var displayOrder = 1;
        foreach (ChapterQuestion reorder in questions.Where(x => x.Id != id).OrderBy(x => x.DisplayOrder))
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

    public async Task<ServiceResult> DeleteChapterSubscription(Guid currentMemberId, Guid id)
    {
        var subscription = await GetChapterSubscription(currentMemberId, id);

        _unitOfWork.ChapterSubscriptionRepository.Delete(subscription);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    public Task<Chapter?> GetChapter(string name)
    {
        return _unitOfWork.ChapterRepository.GetByName(name).RunAsync();
    }

    public async Task<ChapterAdminMemberDto> GetChapterAdminMemberDto(Guid currentMemberId, Guid chapterId, Guid memberId)
    {
        var (chapterAdminMembers, currentMember, member) = await _unitOfWork.RunAsync(
            x => x.ChapterAdminMemberRepository.GetByChapterId(chapterId),
            x => x.MemberRepository.GetById(currentMemberId),
            x => x.MemberRepository.GetById(memberId));

        AssertMemberIsChapterAdmin(currentMember, chapterId, chapterAdminMembers);

        var adminMember = chapterAdminMembers.FirstOrDefault(x => x.MemberId == memberId);
        if (adminMember == null)
        {
            throw new OdkNotFoundException();
        }

        return new ChapterAdminMemberDto
        {
            AdminMember = adminMember,
            Member = member
        };
    }

    public async Task<ChapterAdminMembersDto> GetChapterAdminMemberDtos(Guid currentMemberId, Guid chapterId)
    {
        var (chapterAdminMembers, currentMember, members) = await _unitOfWork.RunAsync(
            x => x.ChapterAdminMemberRepository.GetByChapterId(chapterId),
            x => x.MemberRepository.GetById(currentMemberId),
            x => x.MemberRepository.GetByChapterId(chapterId));
        
        AssertMemberIsChapterAdmin(currentMember, chapterId, chapterAdminMembers);

        return new ChapterAdminMembersDto
        {
            AdminMembers = chapterAdminMembers,
            Members = members
        };
    }

    public async Task<IReadOnlyCollection<ContactRequest>> GetChapterContactRequests(Guid currentMemberId,
        Guid chapterId)
    {
        return await GetChapterAdminRestrictedContent(currentMemberId, chapterId,
            x => x.ContactRequestRepository.GetByChapterId(chapterId));
    }

    public async Task<ChapterEventSettings?> GetChapterEventSettings(Guid currentMemberId, Guid chapterId)
    {
        return await GetChapterAdminRestrictedContent(currentMemberId, chapterId,
            x => x.ChapterEventSettingsRepository.GetByChapterId(chapterId));
    }

    public async Task<ChapterMembershipSettings?> GetChapterMembershipSettings(Guid currentMemberId, Guid chapterId)
    {
        return await GetChapterAdminRestrictedContent(currentMemberId, chapterId,
            x => x.ChapterMembershipSettingsRepository.GetByChapterId(chapterId));
    }

    public async Task<ChapterPaymentSettings?> GetChapterPaymentSettings(Guid currentMemberId, Guid chapterId)
    {
        return await GetChapterAdminRestrictedContent(currentMemberId, chapterId,
            x => x.ChapterPaymentSettingsRepository.GetByChapterId(chapterId));
    }

    public async Task<IReadOnlyCollection<ChapterProperty>> GetChapterProperties(Guid currentMemberId, Guid chapterId)
    {
        return await GetChapterAdminRestrictedContent(currentMemberId, chapterId,
            x => x.ChapterPropertyRepository.GetByChapterId(chapterId));
    }

    public async Task<ChapterProperty> GetChapterProperty(Guid currentMemberId, Guid id)
    {
        var property = await _unitOfWork.ChapterPropertyRepository.GetById(id).RunAsync();
        await AssertMemberIsChapterAdmin(currentMemberId, property.ChapterId);
        return property;
    }

    public async Task<ChapterQuestion> GetChapterQuestion(Guid currentMemberId, Guid questionId)
    {
        var question = await _unitOfWork.ChapterQuestionRepository.GetById(questionId).RunAsync();
        await AssertMemberIsChapterAdmin(currentMemberId, question.ChapterId);
        return question;
    }

    public async Task<ChapterSubscription> GetChapterSubscription(Guid currentMemberId, Guid id)
    {
        var subscription = await _unitOfWork.ChapterSubscriptionRepository.GetById(id).RunAsync();
        await AssertMemberIsChapterAdmin(currentMemberId, subscription.ChapterId);
        return subscription;
    }

    public async Task<IReadOnlyCollection<ChapterSubscription>> GetChapterSubscriptions(Guid currentMemberId, Guid chapterId)
    {
        return await GetChapterAdminRestrictedContent(currentMemberId, chapterId,
            x => x.ChapterSubscriptionRepository.GetByChapterId(chapterId));        
    }

    public async Task<ServiceResult> UpdateChapterAdminMember(Guid currentMemberId, Guid chapterId, Guid memberId,
        UpdateChapterAdminMember model)
    {
        var (chapterAdminMembers, currentMember) = await _unitOfWork.RunAsync(
            x => x.ChapterAdminMemberRepository.GetByChapterId(chapterId),
            x => x.MemberRepository.GetById(currentMemberId));
        AssertMemberIsChapterAdmin(currentMember, chapterId, chapterAdminMembers);

        var existing = chapterAdminMembers.FirstOrDefault(x => x.MemberId == memberId);
        if (existing == null)
        {
            return ServiceResult.Failure("Chapter admin member not found");
        }

        existing.AdminEmailAddress = model.AdminEmailAddress;
        existing.ReceiveContactEmails = model.ReceiveContactEmails;
        existing.ReceiveNewMemberEmails = model.ReceiveNewMemberEmails;
        existing.SendNewMemberEmails = model.SendNewMemberEmails;

        _unitOfWork.ChapterAdminMemberRepository.Update(existing);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    public async Task UpdateChapterLinks(Guid currentMemberId, Guid chapterId, UpdateChapterLinks update)
    {
        var links = await GetChapterAdminRestrictedContent(currentMemberId, chapterId,
            x => x.ChapterLinksRepository.GetByChapterId(chapterId));

        if (links == null)
        {
            links = new ChapterLinks();            
        }

        links.FacebookName = update.Facebook;
        links.TwitterName = update.Twitter;
        links.InstagramName = update.Instagram;        
        
        if (links.ChapterId == default)
        {
            links.ChapterId = chapterId;
            _unitOfWork.ChapterLinksRepository.Add(links);
        }
        else
        {
            _unitOfWork.ChapterLinksRepository.Update(links);
        }

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task UpdateChapterEventSettings(Guid currentMemberId, Guid chapterId, UpdateChapterEventSettings model)
    {
        var settings = await GetChapterAdminRestrictedContent(currentMemberId, chapterId,
            x => x.ChapterEventSettingsRepository.GetByChapterId(chapterId));

        if (settings == null)
        {
            settings = new();
        }

        settings.DefaultDayOfWeek = model.DefaultDayOfWeek;
        settings.DefaultDescription = model.DefaultDescription;
        settings.DefaultScheduledEmailDayOfWeek = model.DefaultScheduledEmailDayOfWeek;
        settings.DefaultScheduledEmailTimeOfDay = model.DefaultScheduledEmailTimeOfDay;

        if (settings.ChapterId == default)
        {
            settings.ChapterId = chapterId;
            _unitOfWork.ChapterEventSettingsRepository.Add(settings);
        }
        else
        {
            _unitOfWork.ChapterEventSettingsRepository.Update(settings);
        }

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<ServiceResult> UpdateChapterMembershipSettings(Guid currentMemberId, Guid chapterId, 
        UpdateChapterMembershipSettings model)
    {
        var settings = await GetChapterAdminRestrictedContent(currentMemberId, chapterId,
            x => x.ChapterMembershipSettingsRepository.GetByChapterId(chapterId));
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
            settings.ChapterId = chapterId;
            _unitOfWork.ChapterMembershipSettingsRepository.Add(settings);
        }
        else
        {
            _unitOfWork.ChapterMembershipSettingsRepository.Update(settings);
        }

        await _unitOfWork.SaveChangesAsync();

        _cacheService.UpdateItem(settings, chapterId);

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> UpdateChapterPaymentSettings(Guid currentMemberId, Guid chapterId,
        UpdateChapterPaymentSettings model)
    {
        var settings = await GetChapterAdminRestrictedContent(currentMemberId, chapterId,
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
    
    public async Task<ServiceResult> UpdateChapterProperty(Guid currentMemberId, Guid propertyId, UpdateChapterProperty model)
    {
        var property = await GetChapterProperty(currentMemberId, propertyId);

        property.HelpText = model.HelpText;
        property.Hidden = model.Hidden;
        property.Label = model.Label;
        property.Name = model.Name.ToLowerInvariant();
        property.Required = model.Required;
        property.Subtitle = model.Subtitle;

        var properties = await _unitOfWork.ChapterPropertyRepository.GetByChapterId(property.ChapterId, true).RunAsync();

        var validationResult = ValidateChapterProperty(property, properties);
        if (!validationResult.Success)
        {
            return validationResult;
        }

        _unitOfWork.ChapterPropertyRepository.Update(property);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    public async Task<IReadOnlyCollection<ChapterProperty>> UpdateChapterPropertyDisplayOrder(Guid currentMemberId, Guid propertyId, int moveBy)
    {
        var property = await GetChapterProperty(currentMemberId, propertyId);
        var properties = await _unitOfWork.ChapterPropertyRepository.GetByChapterId(property.ChapterId).RunAsync();

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

    public async Task<ServiceResult> UpdateChapterQuestion(Guid currentMemberId, Guid questionId, CreateChapterQuestion update)
    {
        var question = await GetChapterQuestion(currentMemberId, questionId);

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

    public async Task<IReadOnlyCollection<ChapterQuestion>> UpdateChapterQuestionDisplayOrder(Guid currentMemberId, Guid questionId, int moveBy)
    {
        var question = await GetChapterQuestion(currentMemberId, questionId);
        var questions = await _unitOfWork.ChapterQuestionRepository.GetByChapterId(question.ChapterId).RunAsync();

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

    public async Task<ServiceResult> UpdateChapterSubscription(Guid currentMemberId, Guid id, CreateChapterSubscription model)
    {
        var subscription = await _unitOfWork.ChapterSubscriptionRepository.GetByIdOrDefault(id).RunAsync();
        if (subscription == null)
        {
            return ServiceResult.Failure("Not found");
        }

        var subscriptions = await GetChapterAdminRestrictedContent(currentMemberId, subscription.ChapterId,
            x => x.ChapterSubscriptionRepository.GetByChapterId(subscription.ChapterId));

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
    
    public async Task<ServiceResult> UpdateChapterTexts(Guid currentMemberId, Guid chapterId,
        UpdateChapterTexts model)
    {
        var texts = await GetChapterAdminRestrictedContent(currentMemberId, chapterId,
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
    
    public async Task<ServiceResult> UpdateChapterTimeZone(Guid currentMemberId, Chapter chapter, string? timeZoneId)
    {
        await AssertMemberIsSuperAdmin(currentMemberId);

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
    
    private ServiceResult ValidateChapterProperty(ChapterProperty property, IReadOnlyCollection<ChapterProperty> existing)
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
