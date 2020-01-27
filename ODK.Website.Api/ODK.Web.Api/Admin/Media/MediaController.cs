using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ODK.Core.Media;
using ODK.Services.Media;
using ODK.Web.Api.Admin.Media.Responses;
using ODK.Web.Common;

namespace ODK.Web.Api.Admin.Members
{
    [Authorize]
    [ApiController]
    [Route("Admin/Media")]
    public class MediaController : OdkControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IMediaAdminService _mediaAdminService;

        public MediaController(IMediaAdminService mediaAdminService, IMapper mapper)
        {
            _mapper = mapper;
            _mediaAdminService = mediaAdminService;
        }

        [HttpGet]
        public async Task<IEnumerable<MediaFileApiResponse>> GetMedia(Guid chapterId)
        {
            IReadOnlyCollection<MediaFile> mediaFiles = await _mediaAdminService.GetMediaFiles(GetMemberId(), chapterId);
            return mediaFiles.Select(_mapper.Map<MediaFileApiResponse>);
        }

        [HttpPost]
        public async Task<IActionResult> UploadMedia(Guid chapterId, [FromForm] IFormFile file)
        {
            byte[] data = await GetFileData(file);
            await _mediaAdminService.SaveMediaFile(GetMemberId(), chapterId, file.Name, data);
            return NoContent();
        }
    }
}
