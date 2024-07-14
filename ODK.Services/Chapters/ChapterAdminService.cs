using ODK.Core.Chapters;
using ODK.Core.DataTypes;
using ODK.Core.Members;
using ODK.Services.Caching;
using ODK.Services.Exceptions;

namespace ODK.Services.Chapters;

public class ChapterAdminService : OdkAdminServiceBase, IChapterAdminService
{
    private readonly ICacheService _cacheService;
    private readonly IChapterRepository _chapterRepository;
    private readonly IChapterService _chapterService;
    private readonly IMemberRepository _memberRepository;

    public ChapterAdminService(IChapterRepository chapterRepository, ICacheService cacheService,
        IChapterService chapterService, IMemberRepository memberRepository)
        : base(chapterRepository)
    {
        _cacheService = cacheService;
        _chapterRepository = chapterRepository;
        _chapterService = chapterService;
        _memberRepository = memberRepository;
    }
    
    public async Task<ServiceResult> AddChapterAdminMember(Guid currentMemberId, string chapterName, Guid memberId)
    {
        Chapter? chapter = await _chapterRepository.GetChapter(chapterName);
        if (chapter == null)
        {
            return ServiceResult.Failure("Chapter not found");
        }

        await AssertMemberIsChapterAdmin(currentMemberId, chapter.Id);

        Member? member = await _memberRepository.GetMemberAsync(memberId);
        if (member == null)
        {
            return ServiceResult.Failure("Member not found");
        }

        ChapterAdminMember? existing = await _chapterRepository.GetChapterAdminMember(chapter.Id, memberId);
        if (existing != null)
        {
            return ServiceResult.Failure("Member is already a chapter admin");
        }

        ChapterAdminMember adminMember = new ChapterAdminMember(chapter.Id, memberId);
        await _chapterRepository.AddChapterAdminMember(adminMember);

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> CreateChapterProperty(Guid currentMemberId, Guid chapterId, CreateChapterProperty property)
    {
        await AssertMemberIsChapterAdmin(currentMemberId, chapterId);

        IReadOnlyCollection<ChapterProperty> existing = await _chapterRepository.GetChapterProperties(chapterId, true);

        int displayOrder = existing.Count > 0 ? existing.Max(x => x.DisplayOrder) + 1 : 1;
        ChapterProperty create = new ChapterProperty(Guid.Empty, chapterId, property.DataType, property.Name.ToLowerInvariant(),
            property.Label, displayOrder, property.Required, property.Subtitle, property.HelpText, property.Hidden);

        ServiceResult validationResult = ValidateChapterProperty(create, existing);
        if (!validationResult.Success)
        {
            return validationResult;
        }

        await _chapterRepository.AddChapterProperty(create);
        _cacheService.RemoveVersionedCollection<ChapterProperty>(chapterId);

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> CreateChapterQuestion(Guid currentMemberId, Guid chapterId, CreateChapterQuestion question)
    {
        await AssertMemberIsChapterAdmin(currentMemberId, chapterId);

        IReadOnlyCollection<ChapterQuestion> existing = await _chapterRepository.GetChapterQuestions(chapterId);

        int displayOrder = existing.Count  > 0 ? existing.Max(x => x.DisplayOrder) + 1 : 1;
        ChapterQuestion create = new ChapterQuestion(Guid.Empty, chapterId, question.Name, question.Answer, displayOrder, 0);

        ServiceResult validationResult = ValidateChapterQuestion(create);
        if (!validationResult.Success)
        {
            return validationResult;
        }

        await _chapterRepository.CreateChapterQuestion(create);

        _cacheService.RemoveVersionedItem<ChapterQuestion>(chapterId);

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> CreateChapterSubscription(Guid currentMemberId, Guid chapterId, CreateChapterSubscription subscription)
    {
        await AssertMemberIsChapterAdmin(currentMemberId, chapterId);

        ChapterSubscription create = new ChapterSubscription(Guid.Empty, chapterId, subscription.Type, subscription.Name,
            subscription.Title, subscription.Description, subscription.Amount, subscription.Months);

        ServiceResult validationResult = await ValidateChapterSubscription(create);
        if (!validationResult.Success)
        {
            return validationResult;
        }

        await _chapterRepository.CreateChapterSubscription(create);

        return ServiceResult.Successful();
    }
    
    public async Task<ServiceResult> DeleteChapterAdminMember(Guid currentMemberId, string chapterName,
        Guid memberId)
    {
        Chapter? chapter = await _chapterRepository.GetChapter(chapterName);
        if (chapter == null)
        {
            return ServiceResult.Failure("Chapter not found");
        }

        ChapterAdminMember? adminMember = await GetChapterAdminMember(currentMemberId, chapter.Id, memberId);
        if (adminMember == null)
        {
            return ServiceResult.Failure("Admin member not found");
        }

        if (adminMember.SuperAdmin)
        {
            return ServiceResult.Failure("Cannot delete a super admin");
        }

        await _chapterRepository.DeleteChapterAdminMember(chapter.Id, memberId);

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> DeleteChapterContactRequest(Guid currentMemberId, Guid id)
    {
        ContactRequest? contactRequest = await _chapterRepository.GetChapterContactRequest(id);
        if (contactRequest == null)
        {
            return ServiceResult.Failure("Contact request not found");
        }

        await AssertMemberIsChapterAdmin(currentMemberId, contactRequest.ChapterId);

        await _chapterRepository.DeleteChapterContactRequest(id);

        return ServiceResult.Successful();
    }

    public async Task DeleteChapterProperty(Guid currentMemberId, Guid id)
    {
        ChapterProperty property = await GetChapterProperty(currentMemberId, id);

        await _chapterRepository.DeleteChapterProperty(id);

        IReadOnlyCollection<ChapterProperty> properties = await _chapterRepository.GetChapterProperties(property.ChapterId, true);
        int displayOrder = 1;
        foreach (ChapterProperty reorder in properties.OrderBy(x => x.DisplayOrder))
        {
            if (reorder.DisplayOrder != displayOrder)
            {
                reorder.DisplayOrder = displayOrder;
                await _chapterRepository.UpdateChapterProperty(reorder);
            }

            displayOrder++;
        }

        _cacheService.RemoveVersionedCollection<ChapterProperty>(property.ChapterId);
    }

    public async Task DeleteChapterQuestion(Guid currentMemberId, Guid id)
    {
        ChapterQuestion question = await GetChapterQuestion(currentMemberId, id);

        await _chapterRepository.DeleteChapterQuestion(id);

        IReadOnlyCollection<ChapterQuestion> questions = await _chapterRepository.GetChapterQuestions(question.ChapterId);
        int displayOrder = 1;
        foreach (ChapterQuestion reorder in questions.OrderBy(x => x.DisplayOrder))
        {
            if (reorder.DisplayOrder != displayOrder)
            {
                reorder.DisplayOrder = displayOrder;
                await _chapterRepository.UpdateChapterQuestion(reorder);
            }

            displayOrder++;
        }

        _cacheService.RemoveVersionedCollection<ChapterQuestion>(question.ChapterId);
    }

    public async Task<ServiceResult> DeleteChapterSubscription(Guid currentMemberId, Guid id)
    {
        ChapterSubscription subscription = await GetChapterSubscription(currentMemberId, id);

        await _chapterRepository.DeleteChapterSubscription(subscription.Id);

        return ServiceResult.Successful();
    }

    public async Task<Chapter?> GetChapter(Guid currentMemberId, Guid chapterId)
    {
        await AssertMemberIsChapterAdmin(currentMemberId, chapterId);

        return await _chapterRepository.GetChapter(chapterId);
    }

    public Task<Chapter?> GetChapter(string name)
    {
        return _chapterRepository.GetChapter(name);
    }

    public async Task<ChapterAdminMember?> GetChapterAdminMember(Guid currentMemberId, Guid chapterId, Guid memberId)
    {
        await AssertMemberIsChapterAdmin(currentMemberId, chapterId);

        ChapterAdminMember? adminMember = await _chapterRepository.GetChapterAdminMember(chapterId, memberId);
        if (adminMember == null)
        {
            throw new OdkNotFoundException();
        }

        return adminMember;
    }

    public async Task<IReadOnlyCollection<ChapterAdminMember>> GetChapterAdminMembers(Guid currentMemberId, Guid chapterId)
    {
        await AssertMemberIsChapterAdmin(currentMemberId, chapterId);

        return await _chapterRepository.GetChapterAdminMembers(chapterId);
    }

    public async Task<IReadOnlyCollection<ContactRequest>> GetChapterContactRequests(Guid currentMemberId,
        Guid chapterId)
    {
        await AssertMemberIsChapterAdmin(currentMemberId, chapterId);

        return await _chapterRepository.GetChapterContactRequests(chapterId);
    }

    public async Task<ChapterMembershipSettings?> GetChapterMembershipSettings(Guid currentMemberId, Guid chapterId)
    {
        await AssertMemberIsChapterAdmin(currentMemberId, chapterId);

        return await _chapterRepository.GetChapterMembershipSettings(chapterId);
    }

    public async Task<ChapterPaymentSettings?> GetChapterPaymentSettings(Guid currentMemberId, Guid chapterId)
    {
        await AssertMemberIsChapterSuperAdmin(currentMemberId, chapterId);

        return await _chapterRepository.GetChapterPaymentSettings(chapterId);
    }

    public async Task<IReadOnlyCollection<ChapterProperty>> GetChapterProperties(Guid currentMemberId, Guid chapterId)
    {
        await AssertMemberIsChapterAdmin(currentMemberId, chapterId);

        return await _chapterRepository.GetChapterProperties(chapterId, true);
    }

    public async Task<ChapterProperty> GetChapterProperty(Guid currentMemberId, Guid id)
    {
        var property = await _chapterRepository.GetChapterProperty(id);
        if (property == null)
        {
            throw new OdkNotFoundException();
        }

        await AssertMemberIsChapterAdmin(currentMemberId, property.ChapterId);

        return property;
    }

    public async Task<ChapterQuestion> GetChapterQuestion(Guid currentMemberId, Guid questionId)
    {
        var question = await _chapterRepository.GetChapterQuestion(questionId);
        if (question == null)
        {
            throw new OdkNotFoundException();
        }

        await AssertMemberIsChapterAdmin(currentMemberId, question.ChapterId);

        return question;
    }

    public async Task<IReadOnlyCollection<Chapter>> GetChapters(Guid currentMemberId)
    {
        IReadOnlyCollection<ChapterAdminMember> chapterAdminMembers = await _chapterRepository.GetChapterAdminMembersByMember(currentMemberId);
        if (chapterAdminMembers.Count == 0)
        {
            throw new OdkNotAuthorizedException();
        }
        IReadOnlyCollection<Chapter> chapters = await _chapterService.GetChapters();
        return chapterAdminMembers
            .Select(x => chapters.Single(chapter => chapter.Id == x.ChapterId))
            .ToArray();
    }

    public async Task<ChapterSubscription> GetChapterSubscription(Guid currentMemberId, Guid id)
    {
        ChapterSubscription? subscription = await _chapterRepository.GetChapterSubscription(id);
        if (subscription == null)
        {
            throw new OdkNotFoundException();
        }

        await AssertMemberIsChapterAdmin(currentMemberId, subscription.ChapterId);

        return subscription;
    }

    public async Task<IReadOnlyCollection<ChapterSubscription>> GetChapterSubscriptions(Guid currentMemberId, Guid chapterId)
    {
        await AssertMemberIsChapterAdmin(currentMemberId, chapterId);

        return await _chapterRepository.GetChapterSubscriptions(chapterId);
    }

    public async Task<ServiceResult> UpdateChapterAdminMember(Guid currentMemberId, Guid chapterId, Guid memberId,
        UpdateChapterAdminMember adminMember)
    {
        await AssertMemberIsChapterAdmin(currentMemberId, chapterId);

        ChapterAdminMember? existing = await _chapterRepository.GetChapterAdminMember(chapterId, memberId);
        if (existing == null)
        {
            return ServiceResult.Failure("Chapter admin member not found");
        }

        existing.AdminEmailAddress = adminMember.AdminEmailAddress;
        existing.ReceiveContactEmails = adminMember.ReceiveContactEmails;
        existing.ReceiveNewMemberEmails = adminMember.ReceiveNewMemberEmails;
        existing.SendNewMemberEmails = adminMember.SendNewMemberEmails;

        await _chapterRepository.UpdateChapterAdminMember(existing);

        return ServiceResult.Successful();
    }

    public async Task UpdateChapterLinks(Guid currentMemberId, Guid chapterId, UpdateChapterLinks links)
    {
        await AssertMemberIsChapterAdmin(currentMemberId, chapterId);

        ChapterLinks update = new ChapterLinks(chapterId, links.Facebook, links.Instagram, links.Twitter, 0);
        await _chapterRepository.UpdateChapterLinks(update);

        _cacheService.RemoveVersionedItem<ChapterLinks>(chapterId);
    }

    public async Task<ServiceResult> UpdateChapterMembershipSettings(Guid currentMemberId, Guid chapterId, 
        UpdateChapterMembershipSettings settings)
    {
        ChapterMembershipSettings update = await GetChapterMembershipSettings(currentMemberId, chapterId) ?? 
                                           new ChapterMembershipSettings(chapterId, 0, 0, false);

        update.Enabled = settings.Enabled;
        update.MembershipDisabledAfterDaysExpired = settings.MembershipDisabledAfterDaysExpired;
        update.TrialPeriodMonths = settings.TrialPeriodMonths;

        ServiceResult validationResult = ValidateChapterMembershipSettings(update);
        if (!validationResult.Success)
        {
            return validationResult;
        }

        await _chapterRepository.UpdateChapterMembershipSettings(update);

        _cacheService.UpdateItem(update, chapterId);

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> UpdateChapterPaymentSettings(Guid currentMemberId, Guid chapterId,
        UpdateChapterPaymentSettings paymentSettings)
    {
        await AssertMemberIsChapterAdmin(currentMemberId, chapterId);

        var existing = await _chapterRepository.GetChapterPaymentSettings(chapterId);
        if (existing == null)
        {
            return ServiceResult.Failure("Payment settings not found");
        }

        var update = new ChapterPaymentSettings(chapterId, 
            paymentSettings.ApiPublicKey, 
            paymentSettings.ApiSecretKey, 
            existing.Provider);

        await _chapterRepository.UpdateChapterPaymentSettings(update);

        return ServiceResult.Successful();
    }
    
    public async Task<ServiceResult> UpdateChapterProperty(Guid currentMemberId, Guid propertyId, UpdateChapterProperty property)
    {
        ChapterProperty update = await GetChapterProperty(currentMemberId, propertyId);

        update.HelpText = property.HelpText;
        update.Hidden = property.Hidden;
        update.Label = property.Label;
        update.Name = property.Name.ToLowerInvariant();
        update.Required = property.Required;
        update.Subtitle = property.Subtitle;

        IReadOnlyCollection<ChapterProperty> existing = await _chapterRepository.GetChapterProperties(update.ChapterId, true);

        ServiceResult validationResult = ValidateChapterProperty(update, existing);
        if (!validationResult.Success)
        {
            return validationResult;
        }

        await _chapterRepository.UpdateChapterProperty(update);

        _cacheService.RemoveVersionedCollection<ChapterProperty>(update.ChapterId);

        return ServiceResult.Successful();
    }

    public async Task<IReadOnlyCollection<ChapterProperty>> UpdateChapterPropertyDisplayOrder(Guid currentMemberId, Guid propertyId, int moveBy)
    {
        ChapterProperty property = await GetChapterProperty(currentMemberId, propertyId);
        IReadOnlyCollection<ChapterProperty> properties = await _chapterRepository.GetChapterProperties(property.ChapterId);

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

        await _chapterRepository.UpdateChapterProperty(property);
        await _chapterRepository.UpdateChapterProperty(switchWith);

        _cacheService.RemoveVersionedCollection<ChapterProperty>(property.ChapterId);

        return properties.OrderBy(x => x.DisplayOrder).ToArray();
    }

    public async Task<ServiceResult> UpdateChapterQuestion(Guid currentMemberId, Guid questionId, CreateChapterQuestion question)
    {
        ChapterQuestion update = await GetChapterQuestion(currentMemberId, questionId);

        update.Answer = question.Answer;
        update.Name = question.Name;

        ServiceResult validationResult = ValidateChapterQuestion(update);
        if (!validationResult.Success)
        {
            return validationResult;
        }

        await _chapterRepository.UpdateChapterQuestion(update);

        _cacheService.RemoveVersionedCollection<ChapterQuestion>(update.ChapterId);

        return ServiceResult.Successful();
    }

    public async Task<IReadOnlyCollection<ChapterQuestion>> UpdateChapterQuestionDisplayOrder(Guid currentMemberId, Guid questionId, int moveBy)
    {
        ChapterQuestion question = await GetChapterQuestion(currentMemberId, questionId);
        IReadOnlyCollection<ChapterQuestion> questions = await _chapterRepository.GetChapterQuestions(question.ChapterId);

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

        await _chapterRepository.UpdateChapterQuestion(question);
        await _chapterRepository.UpdateChapterQuestion(switchWith);

        _cacheService.RemoveVersionedCollection<ChapterQuestion>(question.ChapterId);

        return questions.OrderBy(x => x.DisplayOrder).ToArray();
    }

    public async Task<ServiceResult> UpdateChapterSubscription(Guid currentMemberId, Guid id, CreateChapterSubscription subscription)
    {
        ChapterSubscription? existing = await _chapterRepository.GetChapterSubscription(id);
        if (existing == null)
        {
            return ServiceResult.Failure("Not found");
        }

        await AssertMemberIsChapterAdmin(currentMemberId, existing.ChapterId);

        existing.Amount = subscription.Amount;
        existing.Description = subscription.Description;
        existing.Months = subscription.Months;
        existing.Name = subscription.Name;
        existing.Title = subscription.Title;
        existing.Type = subscription.Type;

        ServiceResult validationResult = await ValidateChapterSubscription(existing);
        if (!validationResult.Success)
        {
            return validationResult;
        }

        await _chapterRepository.UpdateChapterSubscription(existing);

        return ServiceResult.Successful();
    }
    
    public async Task<ServiceResult> UpdateChapterTexts(Guid currentMemberId, string chapterName,
        UpdateChapterTexts texts)
    {
        Chapter? chapter = await _chapterRepository.GetChapter(chapterName);
        if (chapter == null)
        {
            return ServiceResult.Failure("Chapter not found");
        }

        if (!await MemberIsChapterAdmin(currentMemberId, chapter.Id))
        {
            return ServiceResult.Failure("Not permitted");
        }

        if (string.IsNullOrWhiteSpace(texts.RegisterText) ||
            string.IsNullOrWhiteSpace(texts.WelcomeText))
        {
            return ServiceResult.Failure("Some required fields are missing");
        }

        ChapterTexts update = new ChapterTexts(chapter.Id, texts.RegisterText, texts.WelcomeText);

        await _chapterRepository.UpdateChapterTexts(update);

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
    
    private async Task<ServiceResult> ValidateChapterSubscription(ChapterSubscription subscription)
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

        IReadOnlyCollection<ChapterSubscription> existing = await _chapterRepository.GetChapterSubscriptions(subscription.ChapterId);
        if (existing.Any(x => x.Id != subscription.Id && x.Name.Equals(subscription.Name)))
        {
            return ServiceResult.Failure("A subscription with that name already exists");
        }

        return ServiceResult.Successful();
    }
}
