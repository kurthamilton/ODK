using System;
using System.Threading.Tasks;
using ODK.Services;
using ODK.Services.Authentication.OAuth;

namespace ODK.Web.Common.Account;

public interface ILoginHandler
{
    /// <summary>
    /// Signs a second member in alongside whoever is already signed in and switches to them, so a site
    /// admin can move between accounts without re-authenticating. Site admins only.
    /// </summary>
    Task<AuthenticationResult> AddAccount(IMemberServiceRequest request, Guid memberId);

    Task<AuthenticationResult> Login(
        IServiceRequest request, string username, string password, bool rememberMe);

    /// <summary>
    /// Signs every account on the cookie out.
    /// </summary>
    Task Logout();

    /// <summary>
    /// Signs one of the cookie's accounts out, leaving the rest signed in. Switches to the first remaining
    /// account when it was the one being acted as, and signs out entirely when it was the only one.
    /// </summary>
    Task<AuthenticationResult> LogoutAccount(IServiceRequest request, Guid memberId);

    Task<AuthenticationResult> OAuthLogin(IServiceRequest request, OAuthProviderType providerType, string token);

    /// <summary>
    /// Acts as one of the accounts already signed in on the cookie, rebuilding its claims so its roles are
    /// whatever the database says now.
    /// </summary>
    Task<AuthenticationResult> SwitchAccount(IServiceRequest request, Guid memberId);
}
