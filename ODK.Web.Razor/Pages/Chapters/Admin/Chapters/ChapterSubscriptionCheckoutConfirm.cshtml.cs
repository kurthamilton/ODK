using Microsoft.AspNetCore.Mvc;
using ODK.Core.Payments;
using ODK.Services.Chapters;
using ODK.Services.Security;
using ODK.Web.Razor.Models.Feedback;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Chapters;

public class ChapterSubscriptionCheckoutConfirmModel : AdminPageModel
{
    private readonly IChapterAdminService _chapterAdminService;

    public ChapterSubscriptionCheckoutConfirmModel(IChapterAdminService chapterAdminService)
    {
        _chapterAdminService = chapterAdminService;
    }

    public override ChapterAdminSecurable Securable => ChapterAdminSecurable.SiteSubscription;

    public string SessionId { get; set; } = string.Empty;

    public async Task<IActionResult> OnGet(string sessionId)
    {
        SessionId = sessionId;

        var redirectUrl = AdminRoutes.Subscription(Chapter).Path;

        var request = MemberChapterAdminServiceRequest;
        var status = await _chapterAdminService.GetChapterPaymentCheckoutSessionStatus(
            request, sessionId);

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