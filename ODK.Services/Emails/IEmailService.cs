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

    Task SendBulkEmail(
        IChapterServiceRequest request,
        IEnumerable<Member> to,
        string subject,
        string body);

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
        string body);

    Task<ServiceResult> SendEmail(
        IServiceRequest request,
        Chapter? chapter,
        IEnumerable<EmailAddressee> to,
        string subject,
        string body,
        IEmailParameters? parameters);

    Task<ServiceResult> SendMemberEmail(
        IServiceRequest request,
        Chapter? chapter,
        EmailAddressee to,
        string subject,
        string body,
        IEmailParameters? parameters);
}