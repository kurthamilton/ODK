using Microsoft.AspNetCore.Mvc;
using ODK.Core.Payments;
using ODK.Services;
using ODK.Services.Chapters;
using ODK.Services.Security;
using ODK.Web.Common.Feedback;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Chapters;

public class ChapterSubscriptionCheckoutConfirmModel : AdminPageModel
{
    private readonly IChapterAdminService _chapterAdminService;

    public ChapterSubscriptionCheckoutConfirmModel(IChapterAdminService chapterAdminService)
    {
        _chapterAdminService = chapterAdminService;
    }

    public string RedirectUrl { get; private set; } = string.Empty;

    public override ChapterAdminSecurable Securable => ChapterAdminSecurable.SiteSubscription;

    public string? SessionId { get; set; }

    public Guid SubscriptionId { get; set; }

    public async Task<IActionResult> OnGet(Guid id, string sessionId)
    {
        SessionId = sessionId;
        SubscriptionId = id;

        RedirectUrl = AdminRoutes.Subscription(Chapter).Path;

        var request = MemberChapterAdminServiceRequest;
        var status = await _chapterAdminService.GetChapterPaymentCheckoutSessionStatus(
            request, sessionId);

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