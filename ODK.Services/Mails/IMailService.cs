using System.Collections.Generic;
using System.Threading.Tasks;
using ODK.Core.Chapters;
using ODK.Core.Mail;
using ODK.Core.Members;

namespace ODK.Services.Mails
{
    public interface IMailService
    {
        Task<MemberEmail> CreateMemberEmail(Member member, Email email, IDictionary<string, string> parameters);

        Task<bool> SendChapterContactMail(Chapter chapter, IDictionary<string, string> parameters);

        Task<MemberEmail> SendMemberMail(MemberEmail memberEmail, Member member, Email email);

        Task<MemberEmail> SendMemberMail(Member member, Email email, IDictionary<string, string> parameters);

        Task<MemberEmail> SendMemberMail(Member member, EmailType type, IDictionary<string, string> parameters);
    }
}
