using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using ODK.Core.Members;

namespace ODK.Services.Authentication
{
    public interface IAuthenticationService
    {
        Task ActivateAccount(string activationToken, string password);

        Task<ServiceResult> ChangePassword(Guid memberId, string currentPassword, string newPassword);

        Task DeleteRefreshToken(string refreshToken);

        Task<IReadOnlyCollection<Claim>> GetClaims(Member member);

        Task<Member> GetMember(string username, string password);

        Task<AuthenticationToken> Login(string username, string password);

        Task<AuthenticationToken> RefreshToken(string refreshToken);

        Task<ServiceResult> RequestPasswordReset(string emailAddress);

        Task<ServiceResult> ResetPassword(string token, string password);
    }
}
