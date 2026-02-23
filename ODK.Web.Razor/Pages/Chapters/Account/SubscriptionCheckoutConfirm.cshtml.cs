using Microsoft.AspNetCore.Mvc;
using ODK.Core.Payments;
using ODK.Services.Payments;
using ODK.Web.Razor.Models.Feedback;

namespace ODK.Web.Razor.Pages.Chapters.Account;

public class SubscriptionCheckoutConfirmModel : OdkPageModel
{
    private readonly IPaymentService _paymentService;

    public SubscriptionCheckoutConfirmModel(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    public string SessionId { get; set; } = string.Empty;

    public async Task<IActionResult> OnGet(string sessionId)
    {
        SessionId = sessionId;
        
        var redirectUrl = OdkRoutes.Account.Subscription(Chapter);
        
        var request = MemberServiceRequest;
        var status = await _paymentService.GetMemberChapterPaymentCheckoutSessionStatus(
            request, Chapter.Id, sessionId);

        if (status == PaymentStatusType.Complete)
        {
            AddFeedback("Purchase complete", FeedbackType.Success);
            return Redirect(redirectUrl);
        }

        if (status == PaymentStatusType.Expired)
        {
            AddFeedback("Purchase not successful. Please try again", FeedbackType.Error);
            return Redirect(redirectUrl);
        }

        return Page();
    }
}