using ODK.Core.Chapters;
using ODK.Core.Emails;
using ODK.Core.Events;
using ODK.Core.Members;

namespace ODK.Services.Emails;

public interface IEmailService
{
    Task SendBulkEmail(
        ServiceRequest request,
        Chapter chapter,
        IEnumerable<Member> to,
        EmailType type,
        IDictionary<string, string> parameters);

    Task SendBulkEmail(
        ServiceRequest request,
        Chapter chapter,
        IEnumerable<Member> to,
        string subject,
        string body);

    Task SendEventCommentEmail(
        ServiceRequest request,
        Chapter chapter,
        Member? replyToMember,
        EventComment comment,
        IDictionary<string, string> parameters);

    Task<ServiceResult> SendEmail(
        ServiceRequest request,
        Chapter? chapter,
        EmailAddressee to,
        EmailType type,
        IDictionary<string, string> parameters);

    Task<ServiceResult> SendEmail(
        ServiceRequest request,
        Chapter? chapter,
        IEnumerable<EmailAddressee> to,
        EmailType type,
        IDictionary<string, string> parameters);

    Task<ServiceResult> SendEmail(
        ServiceRequest request,
        Chapter? chapter,
        IEnumerable<EmailAddressee> to,
        string subject,
        string body);

    Task<ServiceResult> SendEmail(
        ServiceRequest request,
        Chapter? chapter,
        IEnumerable<EmailAddressee> to,
        string subject,
        string body,
        IDictionary<string, string> parameters);

    Task<ServiceResult> SendMemberEmail(
        ServiceRequest request,
        Chapter? chapter,
        EmailAddressee to,
        string subject,
        string body);

    Task<ServiceResult> SendMemberEmail(
        ServiceRequest request,
        Chapter? chapter,
        EmailAddressee to,
        string subject,
        string body,
        IDictionary<string, string> parameters);
}
