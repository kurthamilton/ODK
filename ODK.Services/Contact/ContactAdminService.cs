using ODK.Core.Messages;
using ODK.Core.Notifications;
using ODK.Core.Web;
using ODK.Data.Core;
using ODK.Services.Contact.ViewModels;
using ODK.Services.Members;

namespace ODK.Services.Contact;

public class ContactAdminService : OdkAdminServiceBase, IContactAdminService
{
    private readonly IHtmlSanitizer _htmlSanitizer;
    private readonly IMemberEmailService _memberEmailService;
    private readonly IUnitOfWork _unitOfWork;

    public ContactAdminService(
        IUnitOfWork unitOfWork,
        IHtmlSanitizer htmlSanitizer,
        IMemberEmailService memberEmailService)
        : base(unitOfWork)
    {
        _htmlSanitizer = htmlSanitizer;
        _memberEmailService = memberEmailService;
        _unitOfWork = unitOfWork;
    }

    public async Task<MessagesAdminPageViewModel> GetMessagesViewModel(Guid currentMemberId)
    {
        var (currentMember, messages) = await _unitOfWork.RunAsync(
            x => x.MemberRepository.GetById(currentMemberId),
            x => x.SiteContactMessageRepository.GetAll());

        AssertMemberIsSiteAdmin(currentMember);

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

        AssertMemberIsSiteAdmin(currentMember);

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