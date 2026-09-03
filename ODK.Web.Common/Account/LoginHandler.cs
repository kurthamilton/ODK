using System;
using System.Collections.Generic;
using System.Linq;
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
using ODK.Services.Exceptions;
using ODK.Services.Members;
using ODK.Web.Common.Extensions;
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

    public async Task<AuthenticationResult> AddAccount(IMemberServiceRequest request, Guid memberId)
    {
        OdkAssertions.MeetsCondition(request.CurrentMember, x => x.SiteAdmin);

        var member = await _unitOfWork.MemberRepository.GetById(memberId).Run();

        return await Login(request, member, GetSignedInMemberIds());
    }

    public async Task<AuthenticationResult> Login(
        IServiceRequest request, string username, string password, bool rememberMe)
    {
        var member = await _authenticationService.GetMember(username, password);
        return await Login(request, member, []);
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

    public async Task<AuthenticationResult> LogoutAccount(IServiceRequest request, Guid memberId)
    {
        var signedInMemberIds = GetSignedInMemberIds();

        // The cookie is the only authority for what can be signed out of it, as it is for a switch.
        if (!signedInMemberIds.Contains(memberId))
        {
            throw new OdkNotAuthorizedException();
        }

        var remainingMemberIds = signedInMemberIds
            .Where(x => x != memberId)
            .ToArray();

        if (remainingMemberIds.Length == 0)
        {
            await Logout();
            return new AuthenticationResult();
        }

        // Signing out somebody other than the member being acted as leaves that member in place.
        var currentMemberId = request.CurrentMemberOrDefault?.Id;
        var nextMemberId = currentMemberId != null && remainingMemberIds.Contains(currentMemberId.Value)
            ? currentMemberId.Value
            : remainingMemberIds[0];

        var member = await _unitOfWork.MemberRepository.GetById(nextMemberId).Run();

        return await Login(request, member, remainingMemberIds);
    }

    public async Task<AuthenticationResult> OAuthLogin(
        IServiceRequest request, OAuthProviderType providerType, string token)
    {
        var provider = _oauthProviderFactory.GetProvider(providerType);
        var oauthUser = await provider.GetUser(token);
        var member = await _memberService.FindMemberByEmailAddress(oauthUser.Email);
        return await Login(request, member, []);
    }

    public async Task<AuthenticationResult> SwitchAccount(IServiceRequest request, Guid memberId)
    {
        var signedInMemberIds = GetSignedInMemberIds();

        // The cookie is the only authority for a switch, so a member it does not list is a request to act
        // as somebody this browser was never signed in as.
        if (!signedInMemberIds.Contains(memberId))
        {
            throw new OdkNotAuthorizedException();
        }

        var member = await _unitOfWork.MemberRepository.GetById(memberId).Run();

        return await Login(request, member, signedInMemberIds);
    }

    private IReadOnlyCollection<Guid> GetSignedInMemberIds()
        => _httpContextAccessor.HttpContext?.User.SignedInMemberIds() ?? [];

    private async Task<AuthenticationResult> Login(
        IServiceRequest request, Member? member, IReadOnlyCollection<Guid> signedInMemberIds)
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

        var claims = await _authenticationService.GetClaims(
            MemberServiceRequest.Create(member, request),
            signedInMemberIds);
        await SetAuthCookie(httpContext, claims);
        return new AuthenticationResult
        {
            Member = member,
            Success = true
        };
    }

    private async Task SetAuthCookie(HttpContext httpContext, IReadOnlyCollection<Claim> claims)
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
