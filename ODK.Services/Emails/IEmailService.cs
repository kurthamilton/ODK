using ODK.Core.Chapters;
using ODK.Core.Emails;
using ODK.Core.Events;
using ODK.Core.Members;

namespace ODK.Services.Emails;

public interface IEmailService
{
    Task SendBulkEmail(
        Chapter chapter, 
        IEnumerable<Member> to, 
        EmailType type, 
        IDictionary<string, string> parameters);

    Task SendBulkEmail(
        Chapter chapter,
        IEnumerable<Member> to,
        string subject,
        string body);

    Task SendEventCommentEmail(
        Chapter chapter, 
        Member? replyToMember, 
        EventComment comment,
        IDictionary<string, string> parameters);

    Task<ServiceResult> SendEmail(
        Chapter? chapter, 
        EmailAddressee to, 
        EmailType type, 
        IDictionary<string, string> parameters);

    Task<ServiceResult> SendEmail(
        Chapter? chapter, 
        IEnumerable<EmailAddressee> to, 
        EmailType type,
        IDictionary<string, string> parameters);

    Task<ServiceResult> SendEmail(
        Chapter? chapter, 
        IEnumerable<EmailAddressee> to, 
        string subject, 
        string body);

    Task<ServiceResult> SendEmail(
        Chapter? chapter, 
        IEnumerable<EmailAddressee> to, 
        string subject, 
        string body, 
        IDictionary<string, string> parameters);

    Task<ServiceResult> SendMemberEmail(
        Chapter? chapter,
        EmailAddressee to, 
        string subject, 
        string body);

    Task<ServiceResult> SendMemberEmail(
        Chapter? chapter, 
        EmailAddressee to, 
        string subject, 
        string body,
        IDictionary<string, string> parameters);
}
