using System;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace ODK.Web.Api
{
    public abstract class OdkControllerBase : ControllerBase
    {
        protected IActionResult Created()
        {
            return StatusCode(201);
        }

        protected ActionResult<T> Created<T>(T response)
        {
            return StatusCode(201, response);
        }

        protected Guid GetMemberId()
        {
            Guid? memberId = TryGetMemberId();
            if (memberId == null)
            {
                throw new Exception("Not found");
            }
            return memberId.Value;
        }

        protected Guid? TryGetMemberId()
        {
            Claim claim = User?.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
            if (claim == null)
            {
                return null;
            }

            return Guid.TryParse(claim.Value, out Guid memberId) ? memberId : new Guid?();
        }
    }
}
