using Microsoft.AspNetCore.Mvc;
using ODK.Web.Common.Extensions;
using ODK.Web.Common.Feedback;

namespace ODK.Web.Razor.Controllers;

public abstract class OdkControllerBase : Controller
{
    protected Guid MemberId => User.MemberIdOrDefault() ?? throw new InvalidOperationException();

    protected void AddFeedback(FeedbackViewModel viewModel)
    {
        TempData!.AddFeedback(viewModel);
    }
    
    protected IActionResult RedirectToReferrer()
    {
        string referrer = Request.Headers["Referer"].ToString();
        return Redirect(referrer);
    }
}
