using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ODK.Services.Members;
using ODK.Web.Common.Feedback;
using ODK.Web.Common.Routes;

namespace ODK.Web.Razor.Controllers;

public class GroupsController : OdkControllerBase
{
    private readonly IMemberService _memberService;

    public GroupsController(IMemberService memberService)
    {
        _memberService = memberService;
    }

    [Authorize]
    [HttpPost("groups/{id:guid}/leave")]
    public async Task<IActionResult> LeaveGroup(Guid id, [FromForm] string reason)
    {
        var result = await _memberService.LeaveChapter(MemberId, id, reason);
        if (result.Success)
        {
            AddFeedback("You have left the group", FeedbackType.Success);
            return Redirect(OdkRoutes2.MemberGroups.Index());
        }

        AddFeedback(result);
        return RedirectToReferrer();
    }
}
