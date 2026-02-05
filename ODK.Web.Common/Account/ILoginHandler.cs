using System;
using System.Threading.Tasks;
using ODK.Services;
using ODK.Services.Authentication.OAuth;

namespace ODK.Web.Common.Account;

public interface ILoginHandler
{
    Task<AuthenticationResult> Impersonate(IMemberServiceRequest request, Guid memberId);

    Task<AuthenticationResult> Login(
        IServiceRequest request, string username, string password, bool rememberMe);

    Task Logout();

    Task<AuthenticationResult> OAuthLogin(IServiceRequest request, OAuthProviderType providerType, string token);
}