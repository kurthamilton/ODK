using System.Threading.Tasks;
using ODK.Services.Authentication.OAuth;

namespace ODK.Web.Common.Account;

public interface ILoginHandler
{
    Task<AuthenticationResult> Login(string username, string password, 
        bool rememberMe);

    Task Logout();

    Task<AuthenticationResult> OAuthLogin(OAuthProviderType providerType, string token);
}
