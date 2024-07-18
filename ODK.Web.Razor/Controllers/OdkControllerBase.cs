using System.Text;
using Microsoft.AspNetCore.Mvc;
using ODK.Core.Utils;
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
    
    protected IActionResult DownloadCsv(IReadOnlyCollection<IReadOnlyCollection<string>> data, string fileName)
    {
        var csv = StringUtils.ToCsv(data);        
        return File(Encoding.UTF8.GetBytes(csv), "text/csv", fileName);
    }

    protected IActionResult RedirectToReferrer()
    {
        string referrer = Request.Headers["Referer"].ToString();
        return Redirect(referrer);
    }
}
