using Microsoft.AspNetCore.Mvc;
using ODK.Core.Payments;
using ODK.Services.Chapters;
using ODK.Web.Common.Feedback;
using ODK.Web.Common.Routes;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Chapters;

public class ChapterSubscriptionCheckoutConfirmModel : AdminPageModel
{
    private readonly IChapterAdminService _chapterAdminService;

    public ChapterSubscriptionCheckoutConfirmModel(IChapterAdminService chapterAdminService)
    {
        _chapterAdminService = chapterAdminService;
    }

    public string RedirectUrl { get; private set; } = string.Empty;

    public string? SessionId { get; set; }

    public Guid SubscriptionId { get; set; }

    public async Task<IActionResult> OnGet(Guid id, string sessionId)
    {
        SessionId = sessionId;
        SubscriptionId = id;

        var chapter = await GetChapter();
        RedirectUrl = OdkRoutes.MemberGroups.GroupSubscription(Platform, chapter);

        var request = await CreateMemberChapterServiceRequest();
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