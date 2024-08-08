using System;
using System.Linq;
using System.Security.Claims;
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
}
