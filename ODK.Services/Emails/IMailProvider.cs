using ODK.Core.Chapters;
using ODK.Core.Emails;

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
        ChapterAdminMember fromAdminMember);
}
