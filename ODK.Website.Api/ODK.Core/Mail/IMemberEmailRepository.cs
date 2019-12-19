using System;
using System.Threading.Tasks;

namespace ODK.Core.Mail
{
    public interface IMemberEmailRepository
    {
        Task<Guid> AddMemberEmail(MemberEmail email);
        
        Task<Email> GetEmail(EmailType type);
    }
}
