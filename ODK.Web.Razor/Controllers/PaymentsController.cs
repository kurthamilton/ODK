using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ODK.Services.Members;
using ODK.Services.Members.ViewModels;
using ODK.Web.Razor.Services;

namespace ODK.Web.Razor.Controllers;

[Authorize]
[ApiController]
public class PaymentsController : OdkControllerBase
{
    private readonly IMemberService _memberService;

    public PaymentsController(
        IRequestStore requestStore,
        IMemberService memberService) 
        : base(requestStore)
    {
        _memberService = memberService;
    }

    [HttpGet("groups/{groupId}/payments/sessions/{id}/status")]
    public async Task<IActionResult> GetSessionStatus(string id, Guid groupId)
    {
        var request = MemberChapterServiceRequest(groupId);
        var status = await _memberService.GetMemberChapterPaymentCheckoutSessionStatus(request, id);
        
        return Ok(new
        {
            status
        });
    }
}
