using Microsoft.AspNetCore.Mvc.Filters;
using ODK.Core.Exceptions;

namespace ODK.Web.Razor.Pages.Chapters;

public abstract class ChapterPageModel2 : OdkPageModel2
{
    public string ChapterName { get; private set; } = null!;
    
    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var chapterName = ChapterPageContext.GetChapterName(HttpContext);
        if (chapterName == null)
        {
            throw new OdkNotFoundException();
        }

        ChapterName = chapterName;

        await base.OnPageHandlerExecutionAsync(context, next);
    }
}
