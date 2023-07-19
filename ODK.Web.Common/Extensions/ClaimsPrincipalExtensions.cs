using System;
using System.Linq;
using System.Security.Claims;

namespace ODK.Web.Common.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static Guid? MemberId(this ClaimsPrincipal user)
        {
            Claim claim = user.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
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
}
