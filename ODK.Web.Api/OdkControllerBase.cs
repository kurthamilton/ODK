using System;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ODK.Services;

namespace ODK.Web.Api
{
    public abstract class OdkControllerBase : ControllerBase
    {
        private static readonly Regex VersionRegex = new Regex(@"^""(?<version>\d+)""$");
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

        protected async Task<ActionResult<TResponse>> HandleVersionedRequest<T, TResponse>(Func<int?, Task<VersionedServiceResult<T>>> getter, Func<T, TResponse> map)
        {
            int? version = null;
            string requestETag = Request.Headers["If-None-Match"].FirstOrDefault();
            if (requestETag != null)
            {
                Match match = VersionRegex.Match(requestETag);
                if (match.Success)
                {
                    version = int.Parse(match.Groups["version"].Value);
                }
            }

            VersionedServiceResult<T> result = await getter(version);

            Response.Headers.Add("ETag", $"\"{result.Version}\"");

            if (version == result.Version)
            {
                return StatusCode((int)HttpStatusCode.NotModified);
            }

            return Ok(map(result.Value));
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
