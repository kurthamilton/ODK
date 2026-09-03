using Microsoft.AspNetCore.Mvc;
using ODK.Web.Razor.Models.Feedback;

namespace ODK.Web.Razor.Pages;

/// <summary>
/// Renders feedback toasts for a script that has some to show, so a post made without leaving the page shows
/// the markup a server-rendered page would have carried rather than a copy of it built in JavaScript.
/// </summary>
public class FeedbackModel : OdkPageModel
{
    [BindProperty(Name = "feedback", SupportsGet = true)]
    public IList<FeedbackQueryViewModel> Feedback { get; set; } = [];

    public void OnGet()
    {
    }
}
