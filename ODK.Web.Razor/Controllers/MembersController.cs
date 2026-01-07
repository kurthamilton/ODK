using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ODK.Core.Members;
using ODK.Services.Members;
using ODK.Web.Razor.Services;

namespace ODK.Web.Razor.Controllers;

[ApiController]
public class MembersController : OdkControllerBase
{
    private readonly IMemberService _memberService;

    public MembersController(
        IMemberService memberService,
        IRequestStore requestStore)
        : base(requestStore)
    {
        _memberService = memberService;
    }

    [Authorize]
    [HttpGet("members/{Id}/avatar")]
    public Task<IActionResult> Avatar(Guid id)
        => HandleVersionedRequest(version => _memberService.GetMemberAvatar(version, id), MemberAvatarResult);

    [Authorize]
    [HttpGet("members/{Id}/image")]
    public Task<IActionResult> Image(Guid id)
        => HandleVersionedRequest(version => _memberService.GetMemberImage(version, id), MemberImageResult);

    protected IActionResult MemberAvatarResult(MemberAvatar? image)
    {
        if (image == null)
        {
            return NoContent();
        }

        return File(image.ImageData, image.MimeType);
    }

    protected IActionResult MemberImageResult(MemberImage? image)
    {
        if (image == null)
        {
            return NoContent();
        }

        return File(image.ImageData, image.MimeType);
    }
}
