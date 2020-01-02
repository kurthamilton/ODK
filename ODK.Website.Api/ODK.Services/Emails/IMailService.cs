using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ODK.Core.Chapters;
using ODK.Core.Emails;
using ODK.Core.Members;

namespace ODK.Services.Emails
{
    public interface IMailService
    {
        Task SendChapterContactMail(Chapter chapter, IDictionary<string, string> parameters);

        Task SendChapterNewMemberAdminMail(Chapter chapter, Member member, IDictionary<string, string> parameters);

        Task SendMail(Guid fromAdminMemberId, Guid toMemberId, string subject, string body);

        Task SendMail(Chapter chapter, string to, EmailType type, IDictionary<string, string> parameters);

        Task SendMemberMail(Chapter chapter, Member member, EmailType type, IDictionary<string, string> parameters);

    }
}
