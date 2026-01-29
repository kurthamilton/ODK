using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ODK.Core.Payments;
using ODK.Services.Payments;
using ODK.Web.Common.Routes;
using ODK.Web.Common.Services;

namespace ODK.Web.Razor.Controllers;

[Authorize]
[ApiController]
public class PaymentsController : OdkControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(
        IRequestStore requestStore,
        IPaymentService paymentService,
        IOdkRoutes odkRoutes)
        : base(requestStore, odkRoutes)
    {
        _paymentService = paymentService;
    }

    [HttpGet("payments/sessions/{id}/status")]
    public async Task<IActionResult> GetSessionStatus(string id, Guid? groupId = null)
    {
        PaymentStatusType status;

        if (groupId != null)
        {
            var request = CreateMemberChapterServiceRequest(groupId.Value);
            status = await _paymentService.GetMemberChapterPaymentCheckoutSessionStatus(request, id);
        }
        else
        {
            status = await _paymentService.GetMemberSitePaymentCheckoutSessionStatus(MemberServiceRequest, id);
        }

        return Ok(new
        {
            status = status.ToString()
        });
    }
}
