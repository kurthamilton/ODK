using System.Collections.Generic;
using System.Threading.Tasks;
using ODK.Core.Mail;
using ODK.Core.Members;

namespace ODK.Services.Mails
{
    public interface IMailService
    {
        Task SendMail(Member member, EmailType type, IDictionary<string, string> parameters);

        Task SendMail(Member member, string subject, string body);
    }
}
