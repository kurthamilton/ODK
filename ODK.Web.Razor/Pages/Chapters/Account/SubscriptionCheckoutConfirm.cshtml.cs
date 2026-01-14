using Microsoft.AspNetCore.Mvc;
using ODK.Core.Payments;
using ODK.Services.Payments;
using ODK.Web.Common.Feedback;
using ODK.Web.Common.Routes;

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
        var chapter = await GetChapter();

        RedirectUrl = OdkRoutes.Account.Subscription(Platform, chapter);
        SessionId = sessionId;
        SubscriptionId = SubscriptionId;

        var request = await CreateMemberChapterServiceRequest();
        var status = await _paymentService.GetMemberChapterPaymentCheckoutSessionStatus(request, sessionId);

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