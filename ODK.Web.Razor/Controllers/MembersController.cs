using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ODK.Services.Members;
using ODK.Web.Common.Routes;
using ODK.Web.Common.Services;
using ODK.Web.Razor.Attributes;

namespace ODK.Web.Razor.Controllers;

[ApiController]
public class MembersController : OdkControllerBase
{
    private readonly IMemberService _memberService;

    public MembersController(
        IMemberService memberService,
        IRequestStore requestStore,
        IOdkRoutes odkRoutes)
        : base(requestStore, odkRoutes)
    {
        _memberService = memberService;
    }

    [SkipRequestStoreMiddleware]
    [Authorize]
    [HttpGet("members/{id:guid}/avatar")]
    public async Task<IActionResult> Avatar(Guid id, [FromQuery(Name = "v")] int? version = null)
    {
        var avatar = await _memberService.GetMemberAvatar(id);
        return CacheableFile(avatar.ImageData, avatar.MimeType, version);
    }
}