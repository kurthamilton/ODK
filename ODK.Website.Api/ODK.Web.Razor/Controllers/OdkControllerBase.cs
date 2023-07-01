using Microsoft.AspNetCore.Mvc;
using ODK.Web.Common.Extensions;
using ODK.Web.Razor.Models.Feedback;

namespace ODK.Web.Razor.Controllers
{
    public abstract class OdkControllerBase : Controller
    {
        protected Guid MemberId => User.MemberId() ?? throw new InvalidOperationException();

        protected void AddFeedback(FeedbackViewModel viewModel)
        {
            int.TryParse(TempData["FeedbackCount"]?.ToString(), out int count);

            string key = $"Feedback[{count}]";
            TempData[$"{key}.Message"] = viewModel.Message;
            TempData[$"{key}.Type"] = viewModel.Type;

            TempData["FeedbackCount"] = count + 1;
        }
        
        protected IActionResult RedirectToReferrer()
        {
            string referrer = Request.Headers["Referer"].ToString();
            return Redirect(referrer);
        }
    }
}
