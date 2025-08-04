using Microsoft.AspNetCore.Mvc.Filters;
using ODK.Core;

namespace ODK.Web.Razor.Pages.Chapters;

public abstract class ChapterPageModel2 : OdkPageModel
{
    public string ChapterName { get; private set; } = null!;
    
    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var chapterName = ChapterPageContext.GetChapterName(HttpContext);
        OdkAssertions.Exists(chapterName, $"Chapter name missing");

        ChapterName = chapterName;

        await base.OnPageHandlerExecutionAsync(context, next);
    }
}
