using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ODK.Services.SocialMedia;
using ODK.Web.Common;

namespace ODK.Web.Api.Admin.Venues
{
    [Authorize]
    [ApiController]
    [Route("Admin/SocialMedia")]
    public class SocialMediaController : OdkControllerBase
    {
        private readonly ISocialMediaAdminService _socialMediaAdminService;

        public SocialMediaController(ISocialMediaAdminService socialMediaAdminService)
        {
            _socialMediaAdminService = socialMediaAdminService;
        }

        [HttpPost("Instagram/Login")]
        public async Task<IActionResult> Login(Guid chapterId)
        {
            await _socialMediaAdminService.InstagramLogin(GetMemberId(), chapterId);
            return NoContent();
        }

        [HttpPost("Instagram/Verify/Trigger")]
        public async Task<IActionResult> InstagramTriggerVerifyCode(Guid chapterId)
        {
            await _socialMediaAdminService.InstagramTriggerAccountVerification(GetMemberId(), chapterId);
            return NoContent();
        }

        [HttpPost("Instagram/Verify/SendCode")]
        public async Task<IActionResult> InstagramSendVerifyCode(Guid chapterId, string code)
        {
            await _socialMediaAdminService.InstagramVerifyAccount(GetMemberId(), chapterId, code);
            return NoContent();
        }
    }
}
