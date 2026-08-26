using ODK.Core;
using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Core.Messages;
using ODK.Core.Notifications;
using ODK.Data.Core;
using ODK.Services.Contact.ViewModels;
using ODK.Services.Emails;
using ODK.Services.Emails.Validation;
using ODK.Services.Exceptions;
using ODK.Services.Members;
using ODK.Services.Members.ViewModels;
using ODK.Services.Notifications;
using ODK.Services.Recaptcha;

namespace ODK.Services.Contact;

public class ContactService : IContactService
{
    private readonly IEmailValidationService _emailValidationService;
    private readonly IMemberEmailService _memberEmailService;
    private readonly INotificationService _notificationService;
    private readonly IRecaptchaService _recaptchaService;
    private readonly IUnitOfWork _unitOfWork;

    public ContactService(
        IRecaptchaService recaptchaService,
        IUnitOfWork unitOfWork,
        INotificationService notificationService,
        IMemberEmailService memberEmailService,
        IEmailValidationService emailValidationService)
    {
        _emailValidationService = emailValidationService;
        _memberEmailService = memberEmailService;
        _notificationService = notificationService;
        _recaptchaService = recaptchaService;
        _unitOfWork = unitOfWork;
    }

    public async Task<ServiceResult> ArchiveChapterConversation(IMemberServiceRequest request, Guid conversationId)
    {
        var currentMember = request.CurrentMember;

        var conversation = await _unitOfWork.ChapterConversationRepository
            .GetById(conversationId)
            .Run();

        OdkAssertions.BelongsToMember(conversation, currentMember.Id);

        if (conversation.ArchivedUtc != null)
        {
            return ServiceResult.Successful();
        }

        conversation.ArchivedUtc = DateTime.UtcNow;
        _unitOfWork.ChapterConversationRepository.Update(conversation);

        await _unitOfWork.SaveChanges();

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> ArchiveSiteConversation(IMemberServiceRequest request, Guid conversationId)
    {
        var conversation = await GetOwnSiteConversation(request, conversationId);

        conversation.ArchivedUtc = DateTime.UtcNow;
        _unitOfWork.SiteConversationRepository.Update(conversation);

        await _unitOfWork.SaveChanges();

        return ServiceResult.Successful();
    }

    public async Task<ContactPageViewModel> GetContactPageViewModel(IServiceRequest request)
    {
        var hasQuestions = await _unitOfWork.SiteQuestionRepository
            .HasQuestions(request.Platform)
            .Run();

        return new ContactPageViewModel
        {
            HasQuestions = hasQuestions
        };
    }

    public async Task<SiteConversationPageViewModel> GetSiteConversationPage(
        IMemberServiceRequest request, Guid conversationId)
    {
        var conversation = await GetOwnSiteConversation(request, conversationId);

        var (messages, otherConversations) = await _unitOfWork.Run(
            x => x.SiteConversationMessageRepository.GetDtosByConversationId(conversationId),
            x => x.SiteConversationRepository.GetDtosByMemberId(request.CurrentMember.Id));

        return new SiteConversationPageViewModel
        {
            Conversation = conversation,
            Messages = messages,
            OtherConversations = otherConversations
                .Where(x => x.Conversation.Id != conversationId)
                .ToArray()
        };
    }

    public async Task<SiteConversationsPageViewModel> GetSiteConversationsPage(
        IMemberServiceRequest request, bool archived)
    {
        var currentMember = request.CurrentMember;

        var conversations = await _unitOfWork.SiteConversationRepository
            .GetDtosByMemberId(currentMember.Id)
            .Run();

        /* Both counts come off the one read rather than two more queries: a member has few enough site
           conversations that fetching them all is cheaper than counting them separately, and the tabs need
           both numbers whichever one is being shown. */
        return new SiteConversationsPageViewModel
        {
            ActiveConversationCount = conversations.Count(x => x.Conversation.ArchivedUtc == null),
            Archived = archived,
            ArchivedConversationCount = conversations.Count(x => x.Conversation.ArchivedUtc != null),
            Conversations = conversations
                .Where(x => (x.Conversation.ArchivedUtc != null) == archived)
                .ToArray(),
            CurrentMember = currentMember
        };
    }

    public async Task<ServiceResult> ReplyToChapterConversation(
        IMemberServiceRequest request, Guid conversationId, string message)
    {
        var (platform, currentMember) = (request.Platform, request.CurrentMember);

        var conversation = await _unitOfWork.ChapterConversationRepository.GetById(conversationId).Run();

        OdkAssertions.BelongsToMember(conversation, currentMember.Id);

        var (chapter, adminMembers, notificationSettings) = await _unitOfWork.Run(
            x => x.ChapterRepository.GetById(platform, conversation.ChapterId),
            x => x.ChapterAdminMemberRepository.GetByChapterId(platform, conversation.ChapterId),
            x => x.MemberNotificationSettingsRepository.GetByChapterId(
                conversation.ChapterId, NotificationType.ConversationOwnerMessage));

        var conversationMessage = new ChapterConversationMessage
        {
            ChapterConversationId = conversationId,
            CreatedUtc = DateTime.UtcNow,
            MemberId = request.CurrentMember.Id,
            ReadByMember = true,
            Text = message
        };

        _unitOfWork.ChapterConversationMessageRepository.Add(conversationMessage);

        _notificationService.AddNewConversationOwnerMessageNotifications(
            conversation,
            adminMembers,
            notificationSettings);

        await _unitOfWork.SaveChanges();

        var addressees = adminMembers
            .Where(x => x.ReceiveContactEmails)
            .Select(x => x.Member);

        await _memberEmailService.SendChapterConversationEmail(
            ChapterServiceRequest.Create(chapter, request),
            conversation,
            conversationMessage,
            addressees.ToArray(),
            isReply: true);

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> ReplyToSiteConversation(
        IMemberServiceRequest request, Guid conversationId, string message)
    {
        var conversation = await GetOwnSiteConversation(request, conversationId);
        var now = DateTime.UtcNow;

        var conversationMessage = _unitOfWork.SiteConversationMessageRepository.Add(new SiteConversationMessage
        {
            CreatedUtc = now,
            FirstReadByMemberUtc = now,
            MemberId = request.CurrentMember.Id,
            SiteConversationId = conversation.Id,
            Text = message
        });

        var siteAdmins = await NotifySiteAdmins(conversation);

        await _unitOfWork.SaveChanges();

        // After the commit: the reply is recorded, and an email cannot be taken back.
        await _memberEmailService.SendSiteConversationEmail(
            request, conversation, conversationMessage, siteAdmins, isReply: true);

        return ServiceResult.Successful();
    }

    public async Task SendChapterContactMessage(
        IChapterServiceRequest request,
        string fromAddress,
        string message,
        string recaptchaToken)
    {
        var (platform, chapter) = (request.Platform, request.Chapter);

        await ValidateRequest(fromAddress, message);

        var result = await _recaptchaService.Verify(recaptchaToken);
        var flagged = !_recaptchaService.Success(result);
        if (flagged)
        {
            message = $"[FLAGGED AS SPAM: {result.Score} / 1.0] {message}";
        }

        var (adminMembers, notificationSettings) = await _unitOfWork.Run(
            x => x.ChapterAdminMemberRepository.GetByChapterId(platform, chapter.Id),
            x => x.MemberNotificationSettingsRepository.GetByChapterId(chapter.Id, NotificationType.ChapterContactMessage));

        var contactMessage = new ChapterContactMessage
        {
            ChapterId = chapter.Id,
            CreatedUtc = DateTime.UtcNow,
            FromAddress = fromAddress,
            Message = message,
            RecaptchaScore = result.Score
        };

        _unitOfWork.ChapterContactMessageRepository.Add(contactMessage);

        if (!flagged)
        {
            _notificationService.AddNewChapterContactMessageNotifications(
            contactMessage,
            adminMembers,
            notificationSettings);
        }

        await _unitOfWork.SaveChanges();

        if (!flagged)
        {
            await _memberEmailService.SendChapterMessage(request, adminMembers, contactMessage);
        }
    }

    public async Task SendSiteContactMessage(
        IServiceRequest request,
        string fromAddress,
        string message,
        string recaptchaToken)
    {
        await ValidateRequest(fromAddress, message);

        var siteAdmins = await _unitOfWork.MemberRepository
            .Query()
            .IsSiteAdmin()
            .GetAll()
            .Run();

        var result = await _recaptchaService.Verify(recaptchaToken);

        var flagged = !_recaptchaService.Success(result);
        if (flagged)
        {
            message = $"[FLAGGED AS SPAM: {result.Score} / 1.0] {message}";
        }

        var contactMessage = new SiteContactMessage
        {
            CreatedUtc = DateTime.UtcNow,
            FromAddress = fromAddress,
            Message = message,
            RecaptchaScore = result.Score
        };

        _unitOfWork.SiteContactMessageRepository.Add(contactMessage);

        await _unitOfWork.SaveChanges();

        if (!flagged)
        {
            await _memberEmailService.SendSiteMessage(request, contactMessage, siteAdmins);
        }
    }

    public async Task<ServiceResult> StartChapterConversation(
        IMemberChapterServiceRequest request,
        string subject,
        string message,
        string recaptchaToken)
    {
        var (platform, chapter, currentMember) = (request.Platform, request.Chapter, request.CurrentMember);

        var (
            privacySettings,
            membershipSettings,
            adminMembers,
            notificationSettings) = await _unitOfWork.Run(
            x => x.ChapterPrivacySettingsRepository.GetByChapterId(chapter.Id),
            x => x.ChapterMembershipSettingsRepository.GetByChapterId(chapter.Id),
            x => x.ChapterAdminMemberRepository.GetByChapterId(platform, chapter.Id),
            x => x.MemberNotificationSettingsRepository.GetByChapterId(chapter.Id, NotificationType.ConversationOwnerMessage));

        var result = await _recaptchaService.Verify(recaptchaToken);

        var now = DateTime.UtcNow;

        var conversation = new ChapterConversation
        {
            ChapterId = chapter.Id,
            CreatedUtc = now,
            MemberId = currentMember.Id,
            Subject = subject
        };

        _unitOfWork.ChapterConversationRepository.Add(conversation);

        var conversationMessage = new ChapterConversationMessage
        {
            ChapterConversationId = conversation.Id,
            CreatedUtc = now,
            MemberId = currentMember.Id,
            ReadByMember = true,
            RecaptchaScore = result.Score,
            Text = message
        };

        _unitOfWork.ChapterConversationMessageRepository.Add(conversationMessage);

        _notificationService.AddNewConversationOwnerMessageNotifications(
            conversation,
            adminMembers,
            notificationSettings);

        await _unitOfWork.SaveChanges();

        var emailMembers = adminMembers
            .Where(x => x.ReceiveContactEmails)
            .Select(x => x.Member);

        await _memberEmailService.SendChapterConversationEmail(
            request,
            conversation,
            conversationMessage,
            emailMembers.ToArray(),
            isReply: false);

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> StartSiteConversation(
        IMemberServiceRequest request, string subject, string message)
    {
        var currentMember = request.CurrentMember;
        var now = DateTime.UtcNow;

        var conversation = _unitOfWork.SiteConversationRepository.Add(new SiteConversation
        {
            CreatedUtc = now,
            MemberId = currentMember.Id,
            Subject = subject
        });

        /* Read by the member from the moment they send it - they are the one who wrote it, so leaving it
           unread would make their own message look like something waiting for them. */
        var conversationMessage = _unitOfWork.SiteConversationMessageRepository.Add(new SiteConversationMessage
        {
            CreatedUtc = now,
            FirstReadByMemberUtc = now,
            MemberId = currentMember.Id,
            SiteConversationId = conversation.Id,
            Text = message
        });

        var siteAdmins = await NotifySiteAdmins(conversation);

        await _unitOfWork.SaveChanges();

        // After the commit: the conversation exists, and an email cannot be taken back.
        await _memberEmailService.SendSiteConversationEmail(
            request, conversation, conversationMessage, siteAdmins, isReply: false);

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> UnarchiveChapterConversation(IMemberServiceRequest request, Guid conversationId)
    {
        var currentMember = request.CurrentMember;

        var conversation = await _unitOfWork.ChapterConversationRepository
            .GetById(conversationId)
            .Run();

        OdkAssertions.BelongsToMember(conversation, currentMember.Id);

        if (conversation.ArchivedUtc == null)
        {
            return ServiceResult.Successful();
        }

        conversation.ArchivedUtc = null;
        _unitOfWork.ChapterConversationRepository.Update(conversation);

        await _unitOfWork.SaveChanges();

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> UnarchiveSiteConversation(IMemberServiceRequest request, Guid conversationId)
    {
        var conversation = await GetOwnSiteConversation(request, conversationId);

        conversation.ArchivedUtc = null;
        _unitOfWork.SiteConversationRepository.Update(conversation);

        await _unitOfWork.SaveChanges();

        return ServiceResult.Successful();
    }

    /// <summary>
    /// The conversation, asserted to belong to the member asking for it. Every site-conversation entry point
    /// goes through here so that the check cannot be forgotten by one of them - a member reaching another
    /// member's thread by id would otherwise read and reply to it.
    /// </summary>
    private async Task<SiteConversation> GetOwnSiteConversation(
        IMemberServiceRequest request, Guid conversationId)
    {
        var conversation = await _unitOfWork.SiteConversationRepository.GetById(conversationId).Run();

        OdkAssertions.BelongsToMember(conversation, request.CurrentMember.Id);

        return conversation;
    }

    /// <summary>
    /// Tells the site's admins a member has written. Staged, not committed - the caller saves the message
    /// and the notifications together, so an admin is never told about something that then rolled back.
    /// </summary>
    /// <remarks>
    /// Two reads rather than one batch: the settings are keyed by member, so who the site admins are has to
    /// be known before theirs can be fetched.
    /// </remarks>
    private async Task<IReadOnlyCollection<Member>> NotifySiteAdmins(SiteConversation conversation)
    {
        var siteAdmins = await _unitOfWork.MemberRepository
            .Query(x => x.IsSiteAdmin())
            .GetAll()
            .Run();

        var notificationSettings = await _unitOfWork.MemberNotificationSettingsRepository
            .GetByMemberIds(
                siteAdmins.Select(x => x.Id).ToArray(),
                NotificationType.SiteConversationMemberMessage)
            .Run();

        _notificationService.AddSiteConversationMemberMessageNotifications(
            conversation, siteAdmins, notificationSettings);

        return siteAdmins;
    }

    private async Task ValidateRequest(string fromAddress, string message)
    {
        if (string.IsNullOrWhiteSpace(fromAddress) || string.IsNullOrWhiteSpace(message))
        {
            throw new OdkServiceException("Email address and message must be provided");
        }

        var emailValidationResult = await _emailValidationService.Validate(fromAddress, EmailValidationLevel.Soft);
        if (!emailValidationResult.Success)
        {
            throw new OdkServiceException(emailValidationResult.Message ?? string.Empty);
        }
    }
}
