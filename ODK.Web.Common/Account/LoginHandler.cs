using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using ODK.Core.Members;
using IAuthenticationService = ODK.Services.Authentication.IAuthenticationService;

namespace ODK.Web.Common.Account;

public class LoginHandler : ILoginHandler
{
    private readonly IAuthenticationService _authenticationService;
    private readonly LoginHandlerSettings _settings; 

    public LoginHandler(IAuthenticationService authenticationService, LoginHandlerSettings settings)
    {
        _authenticationService = authenticationService;
        _settings = settings;
    }

    public async Task<AuthenticationResult> Login(HttpContext httpContext, string username, string password, 
        bool rememberMe)
    {
        Member? member = await _authenticationService.GetMemberAsync(username, password);
        if (member == null)
        {
            return new AuthenticationResult();

        }

        IReadOnlyCollection<Claim> claims = await _authenticationService.GetClaimsAsync(member);
        await SetAuthCookieAsync(httpContext, claims);
        return new AuthenticationResult
        {
            Member = member,
            Success = true
        };
    }

    public async Task Logout(HttpContext httpContext)
    {
        await httpContext.SignOutAsync();
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
