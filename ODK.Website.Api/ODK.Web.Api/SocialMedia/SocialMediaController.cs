using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ODK.Services.SocialMedia;
using ODK.Web.Api.SocialMedia.Responses;

namespace ODK.Web.Api.SocialMedia
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class SocialMediaController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly ISocialMediaService _socialMediaService;

        public SocialMediaController(ISocialMediaService socialMediaService, IMapper mapper)
        {
            _mapper = mapper;
            _socialMediaService = socialMediaService;
        }

        [AllowAnonymous]
        [HttpGet("Instagram")]
        public async Task<IEnumerable<SocialMediaImageApiResponse>> GetInstagramImageUrls(Guid chapterId, int max)
        {
            IReadOnlyCollection<SocialMediaImage> images = await _socialMediaService.GetLatestInstagramImages(chapterId);
            return images
                .Take(max > 0 ? max : int.MaxValue)
                .Select(_mapper.Map<SocialMediaImageApiResponse>);
        }
    }
}
