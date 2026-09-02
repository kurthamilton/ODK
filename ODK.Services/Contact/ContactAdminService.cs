using ODK.Core.Messages;
using ODK.Core.Notifications;
using ODK.Data.Core;
using ODK.Data.Core.Deferred;
using ODK.Services.Contact.ViewModels;
using ODK.Services.Html;
using ODK.Services.Members;
using ODK.Services.Notifications;

namespace ODK.Services.Contact;

public class ContactAdminService : OdkAdminServiceBase, IContactAdminService
{
    private readonly IHtmlValidator _htmlValidator;
    private readonly IMemberEmailService _memberEmailService;
    private readonly INotificationService _notificationService;
    private readonly ContactAdminServiceSettings _settings;
    private readonly IUnitOfWork _unitOfWork;

    public ContactAdminService(
        IUnitOfWork unitOfWork,
        IHtmlValidator htmlValidator,
        IMemberEmailService memberEmailService,
        INotificationService notificationService,
        ContactAdminServiceSettings settings)
        : base(unitOfWork)
    {
        _htmlValidator = htmlValidator;
        _memberEmailService = memberEmailService;
        _notificationService = notificationService;
        _settings = settings;
        _unitOfWork = unitOfWork;
    }

    public async Task<ServiceResult> DeleteSpamMessages(IMemberServiceRequest request)
    {
        var spamThreshold = _settings.ContactMessageRecaptchaScoreThreshold;

        var messages = await GetSiteAdminRestrictedContent(request,
            x => x.SiteContactMessageRepository
                .Query(x => x.ForStatus(MessageStatus.Spam, spamThreshold))
                .GetAll());

        _unitOfWork.SiteContactMessageRepository.DeleteMany(messages);
        await _unitOfWork.SaveChanges();

        return ServiceResult.Successful();
    }

    public async Task<MessagesAdminPageViewModel> GetMessagesViewModel(IMemberServiceRequest request, MessageStatus status)
    {
        var spamThreshold = _settings.ContactMessageRecaptchaScoreThreshold;

        var (messages, unrepliedCount, repliedCount, spamCount) = await GetSiteAdminRestrictedContent(request,
            x => x.SiteContactMessageRepository
                .Query(x => x.ForStatus(status, spamThreshold))
                .GetAll(),
            x => status == MessageStatus.Unreplied
                ? new DefaultDeferredQuery<int>(0)
                : x.SiteContactMessageRepository
                    .Query(x => x.ForStatus(MessageStatus.Unreplied, spamThreshold))
                    .Count(),
            x => status == MessageStatus.Replied
                ? new DefaultDeferredQuery<int>(0)
                : x.SiteContactMessageRepository
                    .Query(x => x.ForStatus(MessageStatus.Replied, spamThreshold))
                    .Count(),
            x => status == MessageStatus.Spam
                ? new DefaultDeferredQuery<int>(0)
                : x.SiteContactMessageRepository
                    .Query(x => x.ForStatus(MessageStatus.Spam, spamThreshold))
                    .Count());

        return new MessagesAdminPageViewModel
        {
            Messages = messages,
            Status = status,
            StatusCounts = new Dictionary<MessageStatus, int>
            {
                { MessageStatus.Unreplied, status == MessageStatus.Unreplied ? messages.Count : unrepliedCount },
                { MessageStatus.Replied, status == MessageStatus.Replied ? messages.Count : repliedCount },
                { MessageStatus.Spam, status == MessageStatus.Spam ? messages.Count : spamCount }
            },
            TimeZone = request.CurrentMember.TimeZone
        };
    }

    public async Task<MessageAdminPageViewModel> GetMessageViewModel(IMemberServiceRequest request, Guid messageId)
    {
        var currentMember = request.CurrentMember;

        var (message, replies, notifications) = await GetSiteAdminRestrictedContent(request,
            x => x.SiteContactMessageRepository.GetById(messageId),
            x => x.SiteContactMessageReplyRepository.GetBySiteContactMessageId(messageId),
            x => x.NotificationRepository.GetUnreadByMemberId(currentMember.Id, NotificationType.ChapterContactMessage, messageId));

        if (notifications.Count > 0)
        {
            _unitOfWork.NotificationRepository.MarkAsRead(notifications);
            await _unitOfWork.SaveChanges();
        }

        return new MessageAdminPageViewModel
        {
            Message = message,
            Replies = replies,
            TimeZone = currentMember.TimeZone
        };
    }

    public async Task<SiteConversationAdminPageViewModel> GetSiteConversationViewModel(
        IMemberServiceRequest request, Guid conversationId)
    {
        var (conversation, messages) = await GetSiteAdminRestrictedContent(request,
            x => x.SiteConversationRepository.GetById(conversationId),
            x => x.SiteConversationMessageRepository.GetDtosByConversationId(conversationId));

        var member = await _unitOfWork.MemberRepository.GetById(conversation.MemberId).Run();

        return new SiteConversationAdminPageViewModel
        {
            Conversation = conversation,
            Member = member,
            Messages = messages
        };
    }

    public async Task<SiteConversationsAdminPageViewModel> GetSiteConversationsViewModel(
        IMemberServiceRequest request, bool archived)
    {
        var (active, archivedConversations) = await GetSiteAdminRestrictedContent(request,
            x => x.SiteConversationRepository.GetDtos(archived: false),
            x => x.SiteConversationRepository.GetDtos(archived: true));

        return new SiteConversationsAdminPageViewModel
        {
            ActiveConversationCount = active.Count,
            Archived = archived,
            ArchivedConversationCount = archivedConversations.Count,
            Conversations = archived ? archivedConversations : active,
            TimeZone = request.CurrentMember.TimeZone
        };
    }

    public async Task<ServiceResult> ReplyToMessage(IMemberServiceRequest request, Guid messageId, string message)
    {
        var currentMember = request.CurrentMember;

        var originalMessage = await _unitOfWork.SiteContactMessageRepository.GetById(messageId).Run();

        AssertMemberIsSiteAdmin(currentMember);

        var sendResult = await _memberEmailService.SendSiteMessageReply(
            request,
            originalMessage,
            message);
        if (!sendResult.Success)
        {
            return sendResult;
        }

        var htmlResult = _htmlValidator.Validate(message, DefaultHtmlValidatorOptions);
        if (!htmlResult.Success)
        {
            return htmlResult;
        }

        var now = DateTime.UtcNow;

        originalMessage.RepliedUtc = now;
        _unitOfWork.SiteContactMessageRepository.Update(originalMessage);

        _unitOfWork.SiteContactMessageReplyRepository.Add(new SiteContactMessageReply
        {
            CreatedUtc = now,
            MessageHtml = message,
            MemberId = currentMember.Id,
            SiteContactMessageId = originalMessage.Id
        });

        await _unitOfWork.SaveChanges();

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> ReplyToSiteConversation(
        IMemberServiceRequest request, Guid conversationId, string message)
    {
        var conversation = await GetSiteAdminRestrictedContent(request,
            x => x.SiteConversationRepository.GetById(conversationId));

        var now = DateTime.UtcNow;

        /* Read by the site the moment an admin sends it: they wrote it, so leaving it unread would put their
           own reply back in their inbox. The member's timestamp stays null until they open it. */
        var conversationMessage = _unitOfWork.SiteConversationMessageRepository.Add(new SiteConversationMessage
        {
            CreatedUtc = now,
            FirstReadBySiteAdminUtc = now,
            MemberId = request.CurrentMember.Id,
            SiteConversationId = conversation.Id,
            Text = message
        });

        /* The member hears about it, and the settings are theirs to have turned off. Staged with the message
           so a notification never outlives a reply that failed to save. */
        var (member, notificationSettings) = await _unitOfWork.Run(
            x => x.MemberRepository.GetById(conversation.MemberId),
            x => x.MemberNotificationSettingsRepository.GetByMemberIds(
                [conversation.MemberId], NotificationType.SiteConversationReplies));

        _notificationService.AddSiteConversationReplyNotification(
            conversation, member, notificationSettings);

        await _unitOfWork.SaveChanges();

        // After the commit: the reply is recorded, and an email cannot be taken back.
        await _memberEmailService.SendSiteConversationEmail(
            request, conversation, conversationMessage, [member], isReply: true);

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> SetMessageAsReplied(IMemberServiceRequest request, Guid messageId)
    {
        var originalMessage = await GetSiteAdminRestrictedContent(request,
            x => x.SiteContactMessageRepository.GetById(messageId));

        originalMessage.RepliedUtc = DateTime.UtcNow;

        _unitOfWork.SiteContactMessageRepository.Update(originalMessage);

        await _unitOfWork.SaveChanges();

        return ServiceResult.Successful();
    }
}
