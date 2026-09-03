using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using ODK.Services.Authentication;
using ODK.Services.Exceptions;

namespace ODK.Web.Common.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static bool Authenticated(this ClaimsPrincipal user) => user.Identity?.IsAuthenticated == true;

    public static Guid MemberId(this ClaimsPrincipal user)
    {
        var memberId = user.MemberIdOrDefault();
        if (memberId == null)
        {
            throw new OdkNotAuthorizedException();
        }
        return memberId.Value;
    }

    public static Guid? MemberIdOrDefault(this ClaimsPrincipal user)
    {
        Claim? claim = user.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
        if (claim == null)
        {
            return null;
        }

        if (Guid.TryParse(claim.Value, out Guid memberId))
        {
            return memberId;
        }

        return null;
    }

    /// <summary>
    /// Every member signed in on the cookie, oldest sign-in first, including the one the request acts as.
    /// </summary>
    public static IReadOnlyCollection<Guid> SignedInMemberIds(this ClaimsPrincipal user)
        => new OdkClaimsUser(user.Claims).SignedInMemberIds;
}
