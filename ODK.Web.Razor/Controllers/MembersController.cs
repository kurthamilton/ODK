using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ODK.Core.Members;
using ODK.Services.Members;

namespace ODK.Web.Razor.Controllers;

[ApiController]
public class MembersController : OdkControllerBase
{
    private readonly IMemberService _memberService;
    
    public MembersController(IMemberService memberService)
    {
        _memberService = memberService;
    }

    [Authorize]
    [HttpGet("Members/{Id}/Avatar")]
    public Task<IActionResult> Avatar(Guid id)
        => HandleVersionedRequest(version => _memberService.GetMemberAvatar(version, id), MemberAvatarResult);

    [Authorize]
    [HttpGet("Members/{Id}/Image")]
    public Task<IActionResult> Image(Guid id)
        => HandleVersionedRequest(version => _memberService.GetMemberImage(version, id), MemberImageResult);

    [Authorize]
    [HttpGet("{Chapter}/Members/{Id}/Avatar")]
    public Task<IActionResult> Avatar(Guid id, string chapter) 
        => HandleVersionedRequest(version => _memberService.GetMemberAvatar(version, id), MemberAvatarResult);

    [Authorize]
    [HttpGet("{Chapter}/Members/{Id}/Image")]
    public Task<IActionResult> Image(Guid id, string chapter)
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
