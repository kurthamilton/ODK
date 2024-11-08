using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using ODK.Core.Platforms;
using ODK.Services.Caching;
using ODK.Services.Members;
using ODK.Web.Common.Extensions;
using ODK.Web.Common.Feedback;
using ODK.Web.Common.Routes;

namespace ODK.Web.Razor.Pages.Chapters.Account;

public class SubscriptionCheckoutConfirmModel : ChapterPageModel
{
    private readonly IMemberService _memberService;
    private readonly IPlatformProvider _platformProvider;

    public SubscriptionCheckoutConfirmModel(
        IRequestCache requestCache,
        IPlatformProvider platformProvider,
        IMemberService memberService) 
        : base(requestCache)
    {
        _memberService = memberService;
        _platformProvider = platformProvider;
    }

    public async Task<IActionResult> OnGet(Guid id, string sessionId)
    {
        var memberId = User.MemberId();
        var result = await _memberService.CompleteChapterSubscriptionCheckoutSession(
            memberId, id, sessionId);

        var platform = _platformProvider.GetPlatform();
        var requestUrl = Request.GetDisplayUrl();

        if (result)
        {
            AddFeedback("Purchase complete", FeedbackType.Success);
        }
        else
        {
            AddFeedback("Purchase not successful. Please try again", FeedbackType.Error);
        }

        var url = OdkRoutes.Account.Subscription(platform, Chapter);
        return Redirect(url);
    }
}
