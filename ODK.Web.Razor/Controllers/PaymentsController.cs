using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ODK.Core.Payments;
using ODK.Services.Payments;
using ODK.Web.Razor.Services;

namespace ODK.Web.Razor.Controllers;

[Authorize]
[ApiController]
public class PaymentsController : OdkControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(
        IRequestStore requestStore,
        IPaymentService paymentService) 
        : base(requestStore)
    {
        _paymentService = paymentService;
    }

    [HttpGet("payments/sessions/{id}/status")]
    public async Task<IActionResult> GetSessionStatus(string id, Guid? groupId = null)
    {
        PaymentStatusType status;

        if (groupId != null)
        {
            var request = MemberChapterServiceRequest(groupId.Value);
            status = await _paymentService.GetMemberChapterPaymentCheckoutSessionStatus(request, id);
        }
        else
        {
            status = await _paymentService.GetMemberPaymentCheckoutSessionStatus(MemberServiceRequest, id);
        }

        return Ok(new
        {
            status = status.ToString()
        });
    }
}
