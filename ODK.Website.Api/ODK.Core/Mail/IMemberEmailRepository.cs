using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ODK.Core.Mail
{
    public interface IMemberEmailRepository
    {
        Task<Guid> AddMemberEmail(MemberEmail email);

        Task AddMemberEventEmail(MemberEventEmail email);

        Task ConfirmMemberEmailRead(Guid memberEmailId);

        Task ConfirmMemberEmailSent(Guid memberEmailId);

        Task<IReadOnlyCollection<MemberEventEmail>> GetChapterEventEmails(Guid chapterId);

        Task<Email> GetEmail(EmailType type);

        Task<IReadOnlyCollection<MemberEventEmail>> GetEventEmails(Guid eventId);
    }
}
