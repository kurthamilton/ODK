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
    
    public async Task<ServiceResult> AddChapterAdminMember(Guid currentMemberId, string chapterName, Guid memberId)
    {
        var (chapter, member) = await _unitOfWork.RunAsync(
            x => x.ChapterRepository.GetByName(chapterName),
            x => x.MemberRepository.GetByIdOrDefault(memberId));

        if (chapter == null)
        {
            return ServiceResult.Failure("Chapter not found");
        }

        if (member == null)
        {
            return ServiceResult.Failure("Member not found");
        }

        var chapterAdminMembers = await _unitOfWork.ChapterAdminMemberRepository.GetByChapterId(chapter.Id).RunAsync();
        AssertMemberIsChapterAdmin(currentMemberId, chapter.Id, chapterAdminMembers);

        var existing = chapterAdminMembers.FirstOrDefault(x => x.MemberId == memberId);
        if (existing != null)
        {
            return ServiceResult.Failure("Member is already a chapter admin");
        }

        var adminMember = new ChapterAdminMember
        {
            ChapterId = chapter.Id,
            MemberId = member.Id
        };

        _unitOfWork.ChapterAdminMemberRepository.Add(adminMember);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> CreateChapterProperty(Guid currentMemberId, Chapter chapter, 
        CreateChapterProperty model)
    {
        var (chapterAdminMembers, existing) = await _unitOfWork.RunAsync(
            x => x.ChapterAdminMemberRepository.GetByChapterId(chapter.Id),
            x => x.ChapterPropertyRepository.GetByChapterId(chapter.Id, all: true));

        AssertMemberIsChapterAdmin(currentMemberId, chapter.Id, chapterAdminMembers);

        var displayOrder = existing.Count > 0 ? existing.Max(x => x.DisplayOrder) + 1 : 1;

        var property = new ChapterProperty
        {
            ChapterId = chapter.Id,
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

    public async Task<ServiceResult> CreateChapterQuestion(Guid currentMemberId, Chapter chapter,
        CreateChapterQuestion model)
    {
        var (chapterAdminMembers, existing) = await _unitOfWork.RunAsync(
            x => x.ChapterAdminMemberRepository.GetByChapterId(chapter.Id),
            x => x.ChapterQuestionRepository.GetByChapterId(chapter.Id));

        AssertMemberIsChapterAdmin(currentMemberId, chapter.Id, chapterAdminMembers);

        var displayOrder = existing.Count  > 0 ? existing.Max(x => x.DisplayOrder) + 1 : 1;
        
        var question = new ChapterQuestion
        {
            Answer = model.Answer,
            ChapterId = chapter.Id,
            DisplayOrder = displayOrder,
            Name = model.Name
        };

        ServiceResult validationResult = ValidateChapterQuestion(question);
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
        var (chapterAdminMembers, existing) = await _unitOfWork.RunAsync(
            x => x.ChapterAdminMemberRepository.GetByChapterId(chapterId),
            x => x.ChapterSubscriptionRepository.GetByChapterId(chapterId));

        AssertMemberIsChapterAdmin(currentMemberId, chapterId, chapterAdminMembers);

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
    
    public async Task<ServiceResult> DeleteChapterAdminMember(Guid currentMemberId, string chapterName,
        Guid memberId)
    {
        var chapter = await _unitOfWork.ChapterRepository.GetByName(chapterName).RunAsync();
        if (chapter == null)
        {
            return ServiceResult.Failure("Chapter not found");
        }

        var (chapterAdminMembers, member) = await _unitOfWork.RunAsync(
            x => x.ChapterAdminMemberRepository.GetByChapterId(chapter.Id),
            x => x.MemberRepository.GetById(memberId));

        AssertMemberIsChapterAdmin(currentMemberId, chapter.Id, chapterAdminMembers);

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
        var (chapterAdminMembers, member) = await _unitOfWork.RunAsync(
            x => x.ChapterAdminMemberRepository.GetByChapterId(chapterId),
            x => x.MemberRepository.GetById(memberId));

        AssertMemberIsChapterAdmin(currentMemberId, chapterId, chapterAdminMembers);

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

    public async Task<IReadOnlyCollection<ChapterAdminMemberDto>> GetChapterAdminMemberDtos(Guid currentMemberId, Guid chapterId)
    {
        var (chapterAdminMembers, members) = await _unitOfWork.RunAsync(
            x => x.ChapterAdminMemberRepository.GetByChapterId(chapterId),
            x => x.MemberRepository.GetAdminMembersByChapterId(chapterId));
        
        AssertMemberIsChapterAdmin(currentMemberId, chapterId, chapterAdminMembers);

        var memberDictionary = members.ToDictionary(x => x.Id);
        return chapterAdminMembers
            .Select(x => new ChapterAdminMemberDto
            {
                AdminMember = x,
                Member = memberDictionary[x.MemberId]
            })
            .ToArray();
    }

    public async Task<IReadOnlyCollection<ContactRequest>> GetChapterContactRequests(Guid currentMemberId,
        Guid chapterId)
    {
        await AssertMemberIsChapterAdmin(currentMemberId, chapterId);
        return await _unitOfWork.ContactRequestRepository.GetByChapterId(chapterId).RunAsync();
    }

    public async Task<ChapterMembershipSettings?> GetChapterMembershipSettings(Guid currentMemberId, Guid chapterId)
    {
        await AssertMemberIsChapterAdmin(currentMemberId, chapterId);
        return await _unitOfWork.ChapterMembershipSettingsRepository.GetByChapterId(chapterId).RunAsync();
    }

    public async Task<ChapterPaymentSettings?> GetChapterPaymentSettings(Guid currentMemberId, Guid chapterId)
    {
        await AssertMemberIsChapterSuperAdmin(currentMemberId, chapterId);
        return await _unitOfWork.ChapterPaymentSettingsRepository.GetByChapterId(chapterId).RunAsync();
    }

    public async Task<IReadOnlyCollection<ChapterProperty>> GetChapterProperties(Guid currentMemberId, Guid chapterId)
    {
        await AssertMemberIsChapterAdmin(currentMemberId, chapterId);
        return await _unitOfWork.ChapterPropertyRepository.GetByChapterId(chapterId, true).RunAsync();
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
        var (chapterAdminMembers, subscriptions) = await _unitOfWork.RunAsync(
            x => x.ChapterAdminMemberRepository.GetByChapterId(chapterId),
            x => x.ChapterSubscriptionRepository.GetByChapterId(chapterId));
        AssertMemberIsChapterAdmin(currentMemberId, chapterId, chapterAdminMembers);
        return subscriptions;
    }

    public async Task<ServiceResult> UpdateChapterAdminMember(Guid currentMemberId, Guid chapterId, Guid memberId,
        UpdateChapterAdminMember model)
    {
        var chapterAdminMembers = await _unitOfWork.ChapterAdminMemberRepository.GetByChapterId(chapterId).RunAsync();
        AssertMemberIsChapterAdmin(currentMemberId, chapterId, chapterAdminMembers);

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

    public async Task UpdateChapterLinks(Guid currentMemberId, Chapter chapter, UpdateChapterLinks update)
    {
        var (chapterAdminMembers, links) = await _unitOfWork.RunAsync(
            x => x.ChapterAdminMemberRepository.GetByChapterId(chapter.Id),
            x => x.ChapterLinksRepository.GetByChapterId(chapter.Id));

        AssertMemberIsChapterAdmin(currentMemberId, chapter.Id, chapterAdminMembers);

        if (links == null)
        {
            links = new ChapterLinks();            
        }

        links.FacebookName = update.Facebook;
        links.TwitterName = update.Twitter;
        links.InstagramName = update.Instagram;        
        
        if (links.ChapterId == Guid.Empty)
        {
            links.ChapterId = chapter.Id;
            _unitOfWork.ChapterLinksRepository.Add(links);
        }
        else
        {
            _unitOfWork.ChapterLinksRepository.Update(links);
        }

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<ServiceResult> UpdateChapterMembershipSettings(Guid currentMemberId, Chapter chapter, 
        UpdateChapterMembershipSettings model)
    {
        var settings = await _unitOfWork.ChapterMembershipSettingsRepository
            .GetByChapterId(chapter.Id)
            .RunAsync();
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

        if (settings.ChapterId == Guid.Empty)
        {
            settings.ChapterId = chapter.Id;
            _unitOfWork.ChapterMembershipSettingsRepository.Add(settings);
        }
        else
        {
            _unitOfWork.ChapterMembershipSettingsRepository.Update(settings);
        }

        await _unitOfWork.SaveChangesAsync();

        _cacheService.UpdateItem(settings, chapter.Id);

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> UpdateChapterPaymentSettings(Guid currentMemberId, Chapter chapter,
        UpdateChapterPaymentSettings update)
    {
        var (chapterAdminMembers, existing) = await _unitOfWork.RunAsync(
            x => x.ChapterAdminMemberRepository.GetByChapterId(chapter.Id),
            x => x.ChapterPaymentSettingsRepository.GetByChapterId(chapter.Id));

        AssertMemberIsChapterAdmin(currentMemberId, chapter.Id, chapterAdminMembers);
        
        if (existing == null)
        {
            return ServiceResult.Failure("Payment settings not found");
        }

        existing.ApiPublicKey = update.ApiPublicKey;
        existing.ApiSecretKey = update.ApiSecretKey;

        _unitOfWork.ChapterPaymentSettingsRepository.Update(existing);
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

        ServiceResult validationResult = ValidateChapterProperty(property, properties);
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

    public async Task<ServiceResult> UpdateChapterSubscription(Guid currentMemberId, Guid id, CreateChapterSubscription update)
    {
        var subscription = await _unitOfWork.ChapterSubscriptionRepository.GetByIdOrDefault(id).RunAsync();
        if (subscription == null)
        {
            return ServiceResult.Failure("Not found");
        }

        var (chapterAdminMembers, subscriptions) = await _unitOfWork.RunAsync(
            x => x.ChapterAdminMemberRepository.GetByChapterId(subscription.ChapterId),
            x => x.ChapterSubscriptionRepository.GetByChapterId(subscription.ChapterId));

        AssertMemberIsChapterAdmin(currentMemberId, subscription.ChapterId, chapterAdminMembers);

        subscription.Amount = update.Amount;
        subscription.Description = update.Description;
        subscription.Months = update.Months;
        subscription.Name = update.Name;
        subscription.Title = update.Title;
        subscription.Type = update.Type;

        var validationResult = ValidateChapterSubscription(subscription, subscriptions);
        if (!validationResult.Success)
        {
            return validationResult;
        }

        _unitOfWork.ChapterSubscriptionRepository.Update(subscription);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }
    
    public async Task<ServiceResult> UpdateChapterTexts(Guid currentMemberId, string chapterName,
        UpdateChapterTexts update)
    {
        var chapter = await _unitOfWork.ChapterRepository.GetByName(chapterName).RunAsync();
        if (chapter == null)
        {
            return ServiceResult.Failure("Chapter not found");
        }

        if (!await MemberIsChapterAdmin(currentMemberId, chapter.Id))
        {
            return ServiceResult.Failure("Not permitted");
        }

        if (string.IsNullOrWhiteSpace(update.RegisterText) ||
            string.IsNullOrWhiteSpace(update.WelcomeText))
        {
            return ServiceResult.Failure("Some required fields are missing");
        }

        var texts = await _unitOfWork.ChapterTextsRepository.GetByChapterId(chapter.Id).RunAsync();
        texts.RegisterText = update.RegisterText;
        texts.WelcomeText = update.WelcomeText;

        _unitOfWork.ChapterTextsRepository.Update(texts);
        await _unitOfWork.SaveChangesAsync();

        _cacheService.RemoveVersionedItem<ChapterTexts>(chapter.Id);

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
