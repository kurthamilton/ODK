using ODK.Core.Chapters;
using ODK.Core.Emails;
using ODK.Core.Members;

namespace ODK.Services.Emails;

public interface IMailProvider
{
    Task SendBulkEmail(Chapter chapter, IEnumerable<EmailAddressee> to, string subject, string body, bool bcc = true);

    Task SendBulkEmail(
        Chapter chapter, 
        IEnumerable<EmailAddressee> to, 
        string subject, 
        string body, 
        ChapterAdminMember? fromAdminMember,
        Member? fromMember,
        bool bcc = true);

    Task<ServiceResult> SendEmail(
        Chapter chapter, 
        EmailAddressee to, 
        string subject, 
        string body);

    Task<ServiceResult> SendEmail(
        Chapter chapter, 
        EmailAddressee to, 
        string subject, 
        string body, 
        ChapterAdminMember fromAdminMember,
        Member fromMember);
}
