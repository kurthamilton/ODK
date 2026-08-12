using ODK.Core.Chapters;
using ODK.Core.Emails;
using ODK.Core.Events;
using ODK.Core.Members;

namespace ODK.Services.Emails;

public interface IEmailService
{
    Task AddEvent(string externalId, string eventName);

    Task SendBulkEmail(
        IChapterServiceRequest request,
        IEnumerable<Member> to,
        EmailType type,
        IEmailParameters? parameters);

    /* The sends below carry their own subject and body, so there is no email row to say who they are
       written for and each states its own recipient type. That is what {title} resolves through - see
       EmailService.Title. */
    Task SendBulkEmail(
        IChapterServiceRequest request,
        IEnumerable<Member> to,
        string subject,
        string body,
        EmailRecipientType recipientType);

    Task SendEventCommentEmail(
        IServiceRequest request,
        Chapter chapter,
        Member? replyToMember,
        EventComment comment,
        IEmailParameters? parameters);

    Task<ServiceResult> SendEmail(
        IServiceRequest request,
        Chapter? chapter,
        EmailAddressee to,
        EmailType type,
        IEmailParameters? parameters);

    Task<ServiceResult> SendEmail(
        IServiceRequest request,
        Chapter? chapter,
        IEnumerable<EmailAddressee> to,
        EmailType type,
        IEmailParameters? parameters);

    Task<ServiceResult> SendEmail(
        IServiceRequest request,
        Chapter? chapter,
        IEnumerable<EmailAddressee> to,
        string subject,
        string body,
        EmailRecipientType recipientType);

    Task<ServiceResult> SendEmail(
        IServiceRequest request,
        Chapter? chapter,
        IEnumerable<EmailAddressee> to,
        string subject,
        string body,
        EmailRecipientType recipientType,
        IEmailParameters? parameters);

    Task<ServiceResult> SendMemberEmail(
        IServiceRequest request,
        Chapter? chapter,
        EmailAddressee to,
        string subject,
        string body,
        EmailRecipientType recipientType,
        IEmailParameters? parameters);
}