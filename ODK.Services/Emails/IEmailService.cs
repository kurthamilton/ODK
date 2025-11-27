using ODK.Core.Chapters;
using ODK.Core.Emails;
using ODK.Core.Events;
using ODK.Core.Members;
using ODK.Core.Web;

namespace ODK.Services.Emails;

public interface IEmailService
{
    Task SendBulkEmail(
        IHttpRequestContext httpRequestContext,
        Chapter chapter, 
        IEnumerable<Member> to, 
        EmailType type, 
        IDictionary<string, string> parameters);

    Task SendBulkEmail(
        IHttpRequestContext httpRequestContext,
        Chapter chapter,
        IEnumerable<Member> to,
        string subject,
        string body);

    Task SendEventCommentEmail(
        IHttpRequestContext httpRequestContext,
        Chapter chapter, 
        Member? replyToMember, 
        EventComment comment,
        IDictionary<string, string> parameters);

    Task<ServiceResult> SendEmail(
        IHttpRequestContext httpRequestContext,
        Chapter? chapter, 
        EmailAddressee to, 
        EmailType type, 
        IDictionary<string, string> parameters);

    Task<ServiceResult> SendEmail(
        IHttpRequestContext httpRequestContext,
        Chapter? chapter, 
        IEnumerable<EmailAddressee> to, 
        EmailType type,
        IDictionary<string, string> parameters);

    Task<ServiceResult> SendEmail(
        IHttpRequestContext httpRequestContext,
        Chapter? chapter, 
        IEnumerable<EmailAddressee> to, 
        string subject, 
        string body);

    Task<ServiceResult> SendEmail(
        IHttpRequestContext httpRequestContext,
        Chapter? chapter, 
        IEnumerable<EmailAddressee> to, 
        string subject, 
        string body, 
        IDictionary<string, string> parameters);

    Task<ServiceResult> SendMemberEmail(
        IHttpRequestContext httpRequestContext,
        Chapter? chapter,
        EmailAddressee to, 
        string subject, 
        string body);

    Task<ServiceResult> SendMemberEmail(
        IHttpRequestContext httpRequestContext,
        Chapter? chapter, 
        EmailAddressee to, 
        string subject, 
        string body,
        IDictionary<string, string> parameters);
}
