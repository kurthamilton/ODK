using System;
using System.Threading.Tasks;

namespace ODK.Services.Authentication
{
    public interface IAuthenticationService
    {
        Task ChangePassword(Guid memberId, string currentPassword, string newPassword);

        Task<AuthenticationToken> Login(string username, string password);

        Task<AuthenticationToken> RefreshToken(string refreshToken);

        Task RequestPasswordReset(string username);       

        Task ResetPassword(string token, string password);
    }
}
