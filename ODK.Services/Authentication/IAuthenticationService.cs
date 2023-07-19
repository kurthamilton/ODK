using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using ODK.Core.Members;

namespace ODK.Services.Authentication
{
    public interface IAuthenticationService
    {
        Task<ServiceResult> ActivateAccount(string activationToken, string password);

        Task<ServiceResult> ChangePassword(Guid memberId, string currentPassword, string newPassword);
        
        Task<IReadOnlyCollection<Claim>> GetClaims(Member member);

        Task<Member> GetMember(string username, string password);
        
        Task<ServiceResult> RequestPasswordReset(string emailAddress);

        Task<ServiceResult> ResetPassword(string token, string password);
    }
}
