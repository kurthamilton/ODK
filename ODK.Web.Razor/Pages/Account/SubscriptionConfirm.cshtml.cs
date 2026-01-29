using Microsoft.AspNetCore.Mvc;
using ODK.Core.Payments;
using ODK.Services.Payments;
using ODK.Web.Common.Feedback;
using ODK.Web.Common.Routes;

namespace ODK.Web.Razor.Pages.Account;

public class SubscriptionConfirmModel : OdkSiteAccountPageModel
{
    private readonly IPaymentService _paymentService;

    public SubscriptionConfirmModel(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    public string RedirectUrl => OdkRoutes.Account.Subscription(null);

    public string? SessionId { get; set; }

    public Guid SubscriptionId { get; private set; }

    public async Task<IActionResult> OnGet(Guid id, string sessionId)
    {
        SessionId = sessionId;
        SubscriptionId = SubscriptionId;

        var request = MemberServiceRequest;
        var status = await _paymentService.GetMemberSitePaymentCheckoutSessionStatus(request, sessionId);

        if (status == PaymentStatusType.Complete)
        {
            AddFeedback("Purchase complete", FeedbackType.Success);
            return Redirect(RedirectUrl);
        }

        if (status == PaymentStatusType.Expired)
        {
            AddFeedback("Purchase not successful. Please try again", FeedbackType.Error);
            return Redirect(RedirectUrl);
        }

        return Page();
    }
}