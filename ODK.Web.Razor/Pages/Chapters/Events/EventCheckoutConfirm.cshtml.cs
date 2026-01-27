using Microsoft.AspNetCore.Mvc;
using ODK.Core.Payments;
using ODK.Services.Payments;
using ODK.Web.Common.Feedback;
using ODK.Web.Common.Routes;

namespace ODK.Web.Razor.Pages.Chapters.Events;

public class EventCheckoutConfirmModel : OdkPageModel
{
    private readonly IPaymentService _paymentService;

    public EventCheckoutConfirmModel(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    public string RedirectUrl { get; private set; } = string.Empty;

    public string? SessionId { get; private set; }

    public string Shortcode { get; private set; } = string.Empty;

    public async Task<IActionResult> OnGet(string shortcode, string sessionId)
    {
        SessionId = sessionId;
        Shortcode = shortcode;

        var chapter = await GetChapter();
        var request = await CreateMemberChapterServiceRequest();
        var status = await _paymentService.GetMemberChapterPaymentCheckoutSessionStatus(request, sessionId);

        RedirectUrl = OdkRoutes.Groups.Event(Platform, chapter, Shortcode);

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