using System;
using System.Threading.Tasks;

namespace ODK.Services.Authentication
{
    public interface IAuthenticationService
    {
        Task ActivateAccount(string activationToken, string password);

        Task ChangePassword(Guid memberId, string currentPassword, string newPassword);

        Task DeleteRefreshToken(string refreshToken);

        Task<AuthenticationToken> Login(string username, string password);

        Task<AuthenticationToken> RefreshToken(string refreshToken);

        Task RequestPasswordReset(string username);

        Task ResetPassword(string token, string password);
    }
}
