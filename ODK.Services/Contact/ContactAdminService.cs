using ODK.Core.Messages;
using ODK.Core.Notifications;
using ODK.Core.Web;
using ODK.Data.Core;
using ODK.Data.Core.Deferred;
using ODK.Services.Contact.ViewModels;
using ODK.Services.Members;

namespace ODK.Services.Contact;

public class ContactAdminService : OdkAdminServiceBase, IContactAdminService
{
    private readonly IHtmlSanitizer _htmlSanitizer;
    private readonly IMemberEmailService _memberEmailService;
    private readonly ContactAdminServiceSettings _settings;
    private readonly IUnitOfWork _unitOfWork;

    public ContactAdminService(
        IUnitOfWork unitOfWork,
        IHtmlSanitizer htmlSanitizer,
        IMemberEmailService memberEmailService,
        ContactAdminServiceSettings settings)
        : base(unitOfWork)
    {
        _htmlSanitizer = htmlSanitizer;
        _memberEmailService = memberEmailService;
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
        await _unitOfWork.SaveChangesAsync();

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
            CurrentMember = request.CurrentMember,
            Messages = messages,
            Status = status,
            StatusCounts = new Dictionary<MessageStatus, int>
            {
                { MessageStatus.Unreplied, status == MessageStatus.Unreplied ? messages.Count : unrepliedCount },
                { MessageStatus.Replied, status == MessageStatus.Replied ? messages.Count : repliedCount },
                { MessageStatus.Spam, status == MessageStatus.Spam ? messages.Count : spamCount }
            }
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
            await _unitOfWork.SaveChangesAsync();
        }

        return new MessageAdminPageViewModel
        {
            CurrentMember = currentMember,
            Message = message,
            Replies = replies
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

        var now = DateTime.UtcNow;

        originalMessage.RepliedUtc = now;
        _unitOfWork.SiteContactMessageRepository.Update(originalMessage);

        _unitOfWork.SiteContactMessageReplyRepository.Add(new SiteContactMessageReply
        {
            CreatedUtc = now,
            Message = _htmlSanitizer.Sanitize(message, DefaultHtmlSantizerOptions) ?? string.Empty,
            MemberId = currentMember.Id,
            SiteContactMessageId = originalMessage.Id
        });

        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> SetMessageAsReplied(IMemberServiceRequest request, Guid messageId)
    {
        var originalMessage = await GetSiteAdminRestrictedContent(request,
            x => x.SiteContactMessageRepository.GetById(messageId));

        originalMessage.RepliedUtc = DateTime.UtcNow;

        _unitOfWork.SiteContactMessageRepository.Update(originalMessage);

        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }
}