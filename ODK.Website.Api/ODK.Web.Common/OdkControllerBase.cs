using System;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ODK.Core.Members;
using ODK.Services;
using ODK.Web.Common.Account.Requests;
using ODK.Web.Common.Extensions;

namespace ODK.Web.Common
{
    public abstract class OdkControllerBase : ControllerBase
    {
        private static readonly Regex VersionRegex = new Regex(@"^""(?<version>-?\d+)""$");

        protected static async Task<UpdateMemberImageApiRequest> FileToApiRequest(IFormFile file)
        {
            byte[] imageData = await GetFileData(file);

            return new UpdateMemberImageApiRequest
            {
                ContentType = file.ContentType,
                ImageData = imageData
            };
        }

        protected static async Task<byte[]> GetFileData(IFormFile file)
        {
            return await file.ToByteArrayAsync();
        }

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

        protected async Task<IActionResult> HandleVersionedRequest<T>(Func<long?, Task<VersionedServiceResult<T>>> getter, Func<T, IActionResult> map)
        {
            int? version = GetRequestVersion();

            VersionedServiceResult<T> result = await getter(version);

            AddVersionHeader(result.Version);

            if (version == result.Version)
            {
                return NotModified();
            }

            return map(result.Value);
        }

        protected async Task<ActionResult<TResponse>> HandleVersionedRequest<T, TResponse>(Func<long?, Task<VersionedServiceResult<T>>> getter, Func<T, TResponse> map)
        {
            int? version = GetRequestVersion();

            VersionedServiceResult<T> result = await getter(version);

            AddVersionHeader(result.Version);

            if (version == result.Version)
            {
                return NotModified();
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

        private void AddVersionHeader(long version)
        {
            Response.Headers.Add("ETag", $"\"{version}\"");
        }

        private int? GetRequestVersion()
        {
            string requestETag = Request.Headers["If-None-Match"].FirstOrDefault();
            if (requestETag == null)
            {
                return null;
            }

            Match match = VersionRegex.Match(requestETag);
            return match.Success ? int.Parse(match.Groups["version"].Value) : new int?();
        }

        private ActionResult NotModified()
        {
            return StatusCode((int)HttpStatusCode.NotModified);
        }

        protected IActionResult MemberImageResult(MemberImage image)
        {
            if (image == null)
            {
                return NoContent();
            }

            return File(image.ImageData, image.MimeType);
        }
    }
}
