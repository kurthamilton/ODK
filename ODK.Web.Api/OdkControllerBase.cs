using System;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace ODK.Web.Api
{
    public abstract class OdkControllerBase : ControllerBase
    {
        public Guid GetMemberId()
        {
            Guid? memberId = TryGetMemberId();
            if (memberId == null)
            {
                throw new Exception("Not found");
            }
            return memberId.Value;
        }

        public Guid? TryGetMemberId()
        {
            if (User == null)
            {
                return null;
            }

            Claim claim = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
            if (claim == null)
            {
                return null;
            }

            return Guid.TryParse(claim.Value, out Guid memberId) ? memberId : new Guid?();
        }        
    }
}
