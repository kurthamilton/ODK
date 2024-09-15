using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using ODK.Core.Members;
using ODK.Services.Authentication.OAuth;
using ODK.Services.Members;
using IAuthenticationService = ODK.Services.Authentication.IAuthenticationService;

namespace ODK.Web.Common.Account;

public class LoginHandler : ILoginHandler
{
    private readonly IAuthenticationService _authenticationService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMemberService _memberService;
    private readonly IOAuthProviderFactory _oauthProviderFactory;
    private readonly LoginHandlerSettings _settings; 

    public LoginHandler(
        IAuthenticationService authenticationService, 
        LoginHandlerSettings settings,
        IHttpContextAccessor httpContextAccessor,
        IOAuthProviderFactory oauthProviderFactory,
        IMemberService memberService)
    {
        _authenticationService = authenticationService;
        _httpContextAccessor = httpContextAccessor;
        _memberService = memberService;
        _oauthProviderFactory = oauthProviderFactory;
        _settings = settings;
    }

    public async Task<AuthenticationResult> Login(string username, string password, 
        bool rememberMe)
    {
        var member = await _authenticationService.GetMemberAsync(username, password);
        return await Login(member);
    }

    public async Task Logout()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            return;
        }

        await httpContext.SignOutAsync();
    }

    public async Task<AuthenticationResult> OAuthLogin(OAuthProviderType providerType, string token)
    {
        var provider = _oauthProviderFactory.GetProvider(providerType);
        var oauthUser = await provider.GetUser(token);
        var member = await _memberService.FindMemberByEmailAddress(oauthUser.Email);
        return await Login(member);
    }

    private async Task<AuthenticationResult> Login(Member? member)
    {
        if (member == null)
        {
            return new AuthenticationResult();
        }

        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            return new AuthenticationResult();
        }

        var claims = await _authenticationService.GetClaimsAsync(member);
        await SetAuthCookieAsync(httpContext, claims);
        return new AuthenticationResult
        {
            Member = member,
            Success = true
        };
    }

    private async Task SetAuthCookieAsync(HttpContext httpContext, IReadOnlyCollection<Claim> claims)
    {
        if (claims.Count == 0)
        {
            return;
        }

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        var authProperties = new AuthenticationProperties
        {
            AllowRefresh = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddDays(_settings.CookieLifetimeDays),
            IsPersistent = true
        };

        await httpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);
    }
}
