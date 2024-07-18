using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ODK.Web.Common.Extensions;
using ODK.Web.Common.Feedback;

namespace ODK.Web.Razor.Pages;

public abstract class OdkPageModel2<T> : PageModel
{
    public string? Title
    {
        get => ViewData["Title"] as string;
        set => ViewData["Title"] = value;
    }

    public T ViewModel { get; private set; } = default!;

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        OnBeforeGetViewModel(context);

        ViewModel = await GetViewModelAsync();
        ViewData["ViewModel"] = ViewModel;

        await base.OnPageHandlerExecutionAsync(context, next);
    }

    protected void AddFeedback(FeedbackViewModel viewModel)
    {
        TempData!.AddFeedback(viewModel);
    }

    protected abstract Task<T> GetViewModelAsync();

    protected virtual void OnBeforeGetViewModel(PageHandlerExecutingContext context)
    {
    }
}
