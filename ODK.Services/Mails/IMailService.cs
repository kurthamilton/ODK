using System.Collections.Generic;
using System.Threading.Tasks;
using ODK.Core.Mail;
using ODK.Core.Members;

namespace ODK.Services.Mails
{
    public interface IMailService
    {
        Task<MemberEmail> CreateMemberEmail(Member member, Email email, IDictionary<string, string> parameters);

        Task<MemberEmail> SendMail(MemberEmail memberEmail, Member member, Email email);

        Task<MemberEmail> SendMail(Member member, Email email, IDictionary<string, string> parameters);

        Task<MemberEmail> SendMail(Member member, EmailType type, IDictionary<string, string> parameters);
    }
}
