using System;
using System.Threading.Tasks;

namespace ODK.Core.Mail
{
    public interface IMemberEmailRepository
    {
        Task<Guid> AddMemberEmail(MemberEmail email);

        Task ConfirmMemberEmailSent(Guid id);

        Task<Email> GetEmail(EmailType type);
    }
}
