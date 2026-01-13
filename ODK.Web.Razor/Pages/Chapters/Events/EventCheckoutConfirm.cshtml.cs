using Microsoft.AspNetCore.Mvc;
using ODK.Core.Chapters;
using ODK.Core.Payments;
using ODK.Services.Payments;
using ODK.Web.Common.Feedback;
using ODK.Web.Common.Routes;

namespace ODK.Web.Razor.Pages.Chapters.Events;

public class EventCheckoutConfirmModel : ChapterPageModel2
{
    private readonly IPaymentService _paymentService;

    public EventCheckoutConfirmModel(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    public Guid EventId { get; private set; }

    public string RedirectUrl { get; private set; } = string.Empty;

    public string? SessionId { get; private set; }

    public async Task<IActionResult> OnGet(Guid id, string sessionId)
    {
        EventId = id;
        SessionId = sessionId;

        var chapter = await GetChapter();
        var request = CreateMemberChapterServiceRequest(chapter.Id);
        var status = await _paymentService.GetMemberChapterPaymentCheckoutSessionStatus(request, sessionId);

        RedirectUrl = OdkRoutes.Groups.Event(Platform, chapter, EventId);

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