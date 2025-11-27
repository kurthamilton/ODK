using Microsoft.AspNetCore.Mvc.Filters;
using ODK.Core;
using ODK.Core.Chapters;

namespace ODK.Web.Razor.Pages.Chapters;

public abstract class ChapterPageModel : OdkPageModel
{    
    public Chapter Chapter { get; private set; } = null!;
    
    public override async Task OnPageHandlerExecutionAsync(
        PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var chapter = await new ChapterPageContext(RequestCache, HttpContext)
            .GetChapterAsync(RequestStore.Platform);
        OdkAssertions.Exists(chapter);

        Chapter = chapter;        

        await base.OnPageHandlerExecutionAsync(context, next);
    }
}
