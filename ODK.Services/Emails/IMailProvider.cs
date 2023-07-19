using System.Collections.Generic;
using System.Threading.Tasks;
using ODK.Core.Chapters;

namespace ODK.Services.Emails
{
    public interface IMailProvider
    {
        Task SendBulkEmail(Chapter chapter, IEnumerable<string> to, string subject, string body, bool bcc = true);

        Task SendBulkEmail(Chapter chapter, IEnumerable<string> to, string subject, string body, ChapterAdminMember from, bool bcc = true);

        Task<ServiceResult> SendEmail(Chapter chapter, string to, string subject, string body);

        Task<ServiceResult> SendEmail(Chapter chapter, string to, string subject, string body, ChapterAdminMember from);
    }
}
