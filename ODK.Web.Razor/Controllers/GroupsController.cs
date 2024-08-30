using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ODK.Core.Platforms;
using ODK.Services.Members;
using ODK.Web.Common.Routes;

namespace ODK.Web.Razor.Controllers;

public class GroupsController : OdkControllerBase
{
    private readonly IMemberService _memberService;
    private readonly IPlatformProvider _platformProvider;

    public GroupsController(IMemberService memberService,
        IPlatformProvider platformProvider)
    {
        _memberService = memberService;
        _platformProvider = platformProvider;
    }

    [Authorize]
    [HttpPost("groups/{id:guid}/leave")]
    public async Task<IActionResult> LeaveGroup(Guid id, [FromForm] string reason)
    {
        var result = await _memberService.LeaveChapter(MemberId, id, reason);
        AddFeedback(result, "You have left the group");
                
        if (!result.Success)
        {
            return RedirectToReferrer();            
        }                

        var platform = _platformProvider.GetPlatform();
        return Redirect(OdkRoutes2.MemberGroups.Index(platform));
    }
}
