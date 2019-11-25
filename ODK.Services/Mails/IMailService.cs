using System.Collections.Generic;
using System.Threading.Tasks;

namespace ODK.Services.Mails
{
    public interface IMailService
    {
        Task SendMail(string from, IEnumerable<string> to, string subject, string body);
    }
}
