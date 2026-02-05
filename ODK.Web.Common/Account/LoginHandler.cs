using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using ODK.Core;
using ODK.Core.Members;
using ODK.Data.Core;
using ODK.Services;
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
    private readonly IUnitOfWork _unitOfWork;

    public LoginHandler(
        IAuthenticationService authenticationService,
        LoginHandlerSettings settings,
        IHttpContextAccessor httpContextAccessor,
        IOAuthProviderFactory oauthProviderFactory,
        IMemberService memberService,
        IUnitOfWork unitOfWork)
    {
        _authenticationService = authenticationService;
        _httpContextAccessor = httpContextAccessor;
        _memberService = memberService;
        _oauthProviderFactory = oauthProviderFactory;
        _settings = settings;
        _unitOfWork = unitOfWork;
    }

    public async Task<AuthenticationResult> Impersonate(IMemberServiceRequest request, Guid memberId)
    {
        var currentMember = request.CurrentMember;
        var member = await _unitOfWork.MemberRepository.GetById(memberId).Run();

        OdkAssertions.MeetsCondition(currentMember, x => x.SiteAdmin);

        await Logout();
        return await Login(request, member);
    }

    public async Task<AuthenticationResult> Login(
        IServiceRequest request, string username, string password, bool rememberMe)
    {
        var member = await _authenticationService.GetMemberAsync(username, password);
        return await Login(request, member);
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

    public async Task<AuthenticationResult> OAuthLogin(
        IServiceRequest request, OAuthProviderType providerType, string token)
    {
        var provider = _oauthProviderFactory.GetProvider(providerType);
        var oauthUser = await provider.GetUser(token);
        var member = await _memberService.FindMemberByEmailAddress(oauthUser.Email);
        return await Login(request, member);
    }

    private async Task<AuthenticationResult> Login(IServiceRequest request, Member? member)
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

        var claims = await _authenticationService.GetClaimsAsync(
            MemberServiceRequest.Create(member, request));
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