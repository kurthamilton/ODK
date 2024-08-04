using Microsoft.AspNetCore.Mvc.RazorPages;
using ODK.Web.Common.Extensions;
using ODK.Web.Common.Feedback;

namespace ODK.Web.Razor.Pages;

public abstract class OdkPageModel2 : PageModel
{
    public Guid MemberId => User.MemberId();

    public Guid? MemberIdOrDefault => User.MemberIdOrDefault();

    public string? Title
    {
        get => ViewData["Title"] as string;
        set => ViewData["Title"] = value;
    }

    protected void AddFeedback(FeedbackViewModel viewModel)
    {
        TempData!.AddFeedback(viewModel);
    }

    protected void AddFeedback(string message, FeedbackType type = FeedbackType.Success)
    {
        AddFeedback(new FeedbackViewModel(message, type));
    }
}
