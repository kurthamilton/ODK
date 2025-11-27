using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using ODK.Core.Platforms;
using ODK.Services.Caching;
using ODK.Services.Subscriptions;
using ODK.Web.Common.Extensions;
using ODK.Web.Common.Feedback;
using ODK.Web.Common.Routes;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Chapters;

public class ChapterSubscriptionCheckoutConfirmModel : AdminPageModel
{
    private readonly ISiteSubscriptionService _siteSubscriptionService;

    public ChapterSubscriptionCheckoutConfirmModel(
        IRequestCache requestCache,
        ISiteSubscriptionService siteSubscriptionService) 
        : base(requestCache)
    {
        _siteSubscriptionService = siteSubscriptionService;
    }

    public async Task<IActionResult> OnGet(Guid id, string sessionId)
    {
        var chapter = await LoadChapter();

        var url = OdkRoutes.MemberGroups.GroupSubscription(Platform, chapter);

        var memberId = chapter.OwnerId;
        if (memberId == null)
        {
            return Redirect(url);
        }

        var result = await _siteSubscriptionService.CompleteSiteSubscriptionCheckoutSession(
            memberId.Value, id, sessionId);

        var requestUrl = Request.GetDisplayUrl();

        if (result)
        {
            AddFeedback("Purchase complete", FeedbackType.Success);
        }
        else
        {
            AddFeedback("Purchase not successful. Please try again", FeedbackType.Error);
        }
        
        return Redirect(url);
    }
}
