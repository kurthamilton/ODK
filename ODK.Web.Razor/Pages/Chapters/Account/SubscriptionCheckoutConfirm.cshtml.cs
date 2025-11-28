using Microsoft.AspNetCore.Mvc;
using ODK.Core.Payments;
using ODK.Services.Members;
using ODK.Web.Common.Feedback;
using ODK.Web.Common.Routes;

namespace ODK.Web.Razor.Pages.Chapters.Account;

public class SubscriptionCheckoutConfirmModel : ChapterPageModel
{
    private readonly IMemberService _memberService;

    public SubscriptionCheckoutConfirmModel(IMemberService memberService)
    {
        _memberService = memberService;
    }

    public string RedirectUrl => OdkRoutes.Account.Subscription(Platform, Chapter);

    public string? SessionId { get; set; }

    public Guid SubscriptionId { get; set; }

    public async Task<IActionResult> OnGet(Guid id, string sessionId)
    {
        SessionId = sessionId;
        SubscriptionId = SubscriptionId;

        var request = MemberChapterServiceRequest(Chapter.Id);
        var status = await _memberService.GetMemberChapterPaymentCheckoutSessionStatus(request, sessionId);

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
