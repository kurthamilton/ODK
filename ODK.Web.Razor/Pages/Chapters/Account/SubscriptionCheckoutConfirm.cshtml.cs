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

    public string RedirectUrl { get; private set; } = string.Empty;

    public string? SessionId { get; set; }

    public Guid SubscriptionId { get; set; }

    public async Task<IActionResult> OnGet(Guid id, string sessionId)
    {
        var chapter = Chapter;

        RedirectUrl = OdkRoutes.Account.Subscription(Chapter);
        SessionId = sessionId;
        SubscriptionId = SubscriptionId;

        var request = MemberServiceRequest;
        var status = await _paymentService.GetMemberChapterPaymentCheckoutSessionStatus(
            request, Chapter.Id, sessionId);

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