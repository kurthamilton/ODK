using ODK.Core.Emails;
using ODK.Core.Messages;
using ODK.Core.Notifications;
using ODK.Core.Web;
using ODK.Data.Core;
using ODK.Services.Contact.ViewModels;
using ODK.Services.Emails;

namespace ODK.Services.Contact;

public class ContactAdminService : OdkAdminServiceBase, IContactAdminService
{
    private readonly IEmailService _emailService;
    private readonly IHtmlSanitizer _htmlSanitizer;
    private readonly IUnitOfWork _unitOfWork;

    public ContactAdminService(
        IUnitOfWork unitOfWork,
        IEmailService emailService,
        IHtmlSanitizer htmlSanitizer)
        : base(unitOfWork)
    {
        _emailService = emailService;
        _htmlSanitizer = htmlSanitizer;
        _unitOfWork = unitOfWork;
    }

    public async Task<MessagesAdminPageViewModel> GetMessagesViewModel(Guid currentMemberId)
    {
        var (currentMember, messages) = await _unitOfWork.RunAsync(
            x => x.MemberRepository.GetById(currentMemberId),
            x => x.SiteContactMessageRepository.GetAll());

        AssertMemberIsSuperAdmin(currentMember);

        return new MessagesAdminPageViewModel
        {
            CurrentMember = currentMember,
            Messages = messages
        };
    }

    public async Task<MessageAdminPageViewModel> GetMessageViewModel(Guid currentMemberId, Guid messageId)
    {
        var (currentMember, message, replies, notifications) = await _unitOfWork.RunAsync(
            x => x.MemberRepository.GetById(currentMemberId),
            x => x.SiteContactMessageRepository.GetById(messageId),
            x => x.SiteContactMessageReplyRepository.GetBySiteContactMessageId(messageId),
            x => x.NotificationRepository.GetUnreadByMemberId(currentMemberId, NotificationType.ChapterContactMessage, messageId));

        AssertMemberIsSuperAdmin(currentMember);

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

    public async Task<ServiceResult> ReplyToMessage(Guid currentMemberId, Guid messageId, string message)
    {
        var (currentMember, originalMessage) = await _unitOfWork.RunAsync(
            x => x.MemberRepository.GetById(currentMemberId),
            x => x.SiteContactMessageRepository.GetById(messageId));

        AssertMemberIsSuperAdmin(currentMember);
        
        var to = new[]
        {
            new EmailAddressee(originalMessage.FromAddress, "")
        };
        var body = $"{message}<hr/><p>Your original message:</p><p>{originalMessage.Message}</p>";

        var sendResult = await _emailService.SendEmail(null, to, "Re: your message to {title}", body);
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
            Message = _htmlSanitizer.Sanitize(message) ?? "",
            MemberId = currentMemberId,
            SiteContactMessageId = originalMessage.Id            
        });

        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> SetMessageAsReplied(Guid currentMemberId, Guid messageId)
    {
        var (currentMember, originalMessage) = await _unitOfWork.RunAsync(
            x => x.MemberRepository.GetById(currentMemberId),
            x => x.SiteContactMessageRepository.GetById(messageId));
        
        originalMessage.RepliedUtc = DateTime.UtcNow;

        _unitOfWork.SiteContactMessageRepository.Update(originalMessage);

        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }
}
