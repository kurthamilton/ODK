using Microsoft.AspNetCore.Mvc;
using ODK.Core.Payments;
using ODK.Services.Payments;
using ODK.Web.Razor.Models.Feedback;

namespace ODK.Web.Razor.Pages.Chapters.Events;

public class EventCheckoutConfirmModel : OdkPageModel
{
    private readonly IPaymentService _paymentService;

    public EventCheckoutConfirmModel(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    public string SessionId { get; private set; } = string.Empty;

    public string Shortcode { get; private set; } = string.Empty;

    public async Task<IActionResult> OnGet(string shortcode, string sessionId)
    {
        SessionId = sessionId;
        Shortcode = shortcode;
        
        var request = MemberServiceRequest;
        var status = await _paymentService.GetMemberChapterPaymentCheckoutSessionStatus(
            request, Chapter.Id, sessionId);

        var redirectUrl = OdkRoutes.Groups.Event(Chapter, Shortcode);

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