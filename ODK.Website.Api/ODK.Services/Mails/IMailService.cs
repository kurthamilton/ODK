using System.Collections.Generic;
using System.Threading.Tasks;
using ODK.Core.Chapters;
using ODK.Core.Mail;
using ODK.Core.Members;

namespace ODK.Services.Mails
{
    public interface IMailService
    {
        Task SendChapterContactMail(Chapter chapter, IDictionary<string, string> parameters);

        Task SendMemberMail(Member member, EmailType type, IDictionary<string, string> parameters);
    }
}
