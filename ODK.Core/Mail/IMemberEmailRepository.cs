using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ODK.Core.Mail
{
    public interface IMemberEmailRepository
    {
        Task<Guid> AddMemberEmail(MemberEmail email);

        Task AddMemberEventEmail(MemberEventEmail email);

        Task ConfirmMemberEmailSent(Guid id);

        Task<Email> GetEmail(EmailType type);

        Task<IReadOnlyCollection<MemberEventEmail>> GetMemberEventEmails(Guid eventId);
    }
}
