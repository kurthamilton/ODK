using Microsoft.AspNetCore.Mvc.Filters;
using ODK.Core;
using ODK.Web.Razor.Pages.Chapters;

namespace ODK.Web.Razor.Pages.Groups;

public abstract class OdkGroupAdminPageModel : OdkPageModel
{
    public Guid ChapterId { get; private set; }

    public override async Task OnPageHandlerExecutionAsync(
        PageHandlerExecutingContext context, 
        PageHandlerExecutionDelegate next)
    {
        var chapterId = ChapterPageContext.GetChapterId(HttpContext);
        OdkAssertions.Exists(chapterId);

        ChapterId = chapterId.Value;

        await base.OnPageHandlerExecutionAsync(context, next);
    }
}
