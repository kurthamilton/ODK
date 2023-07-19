using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using ODK.Core.Media;
using ODK.Services.Media;
using ODK.Web.Common;

namespace ODK.Web.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class MediaController : OdkControllerBase
    {
        private readonly IMediaService _mediaService;

        public MediaController(IMediaService mediaService)
        {
            _mediaService = mediaService;
        }

        [AllowAnonymous]
        [HttpGet("{chapter}/{name}")]
        public async Task<IActionResult> GetMediaFile(string chapter, string name)
        {
            (MediaFile file, byte[] data) = await _mediaService.GetMediaFile(chapter, name);

            new FileExtensionContentTypeProvider().TryGetContentType(file.FilePath, out string contentType);

            return File(data, contentType);
        }
    }
}
