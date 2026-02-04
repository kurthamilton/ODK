using System;
using System.Threading.Tasks;
using ODK.Services;
using ODK.Services.Authentication.OAuth;

namespace ODK.Web.Common.Account;

public interface ILoginHandler
{
    Task<AuthenticationResult> Impersonate(MemberServiceRequest request, Guid memberId);

    Task<AuthenticationResult> Login(
        ServiceRequest request, string username, string password, bool rememberMe);

    Task Logout();

    Task<AuthenticationResult> OAuthLogin(ServiceRequest request, OAuthProviderType providerType, string token);
}
