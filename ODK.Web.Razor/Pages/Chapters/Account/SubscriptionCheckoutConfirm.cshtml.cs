using Microsoft.AspNetCore.Mvc;
using ODK.Web.Common.Extensions;
using ODK.Web.Common.Routes;

namespace ODK.Web.Razor.Pages.Chapters.Account;

public class SubscriptionCheckoutConfirmModel : ChapterPageModel
{
    public async Task<IActionResult> OnGet(Guid id, string sessionId)
    {
        var memberId = User.MemberId();
        //var result = await _memberService.CompleteChapterSubscriptionCheckoutSession(
        //    memberId, id, sessionId);

        //var requestUrl = Request.GetDisplayUrl();

        //if (result)
        //{
        //    AddFeedback("Purchase complete", FeedbackType.Success);
        //}
        //else
        //{
        //    AddFeedback("Purchase not successful. Please try again", FeedbackType.Error);
        //}

        var url = OdkRoutes.Account.Subscription(Platform, Chapter);
        return Redirect(url);
    }
}
