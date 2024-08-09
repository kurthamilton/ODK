using Microsoft.AspNetCore.Mvc.RazorPages;
using ODK.Services;
using ODK.Web.Common.Extensions;
using ODK.Web.Common.Feedback;

namespace ODK.Web.Razor.Pages;

public abstract class OdkPageModel : PageModel
{
    public Guid CurrentMemberId => User.MemberId();

    public Guid? CurrentMemberIdOrDefault => User.MemberIdOrDefault();

    public string? Title
    {
        get => ViewData["Title"] as string;
        set => ViewData["Title"] = value;
    }

    protected void AddFeedback(FeedbackViewModel viewModel) => TempData!.AddFeedback(viewModel);

    protected void AddFeedback(string message, FeedbackType type = FeedbackType.Success)
        => AddFeedback(new FeedbackViewModel(message, type));

    protected void AddFeedback(ServiceResult result) 
        => AddFeedback(new FeedbackViewModel(result));
}
