using Microsoft.AspNetCore.Mvc.Filters;
using ODK.Core;
using ODK.Core.Chapters;
using ODK.Services.Caching;

namespace ODK.Web.Razor.Pages.Chapters;

public abstract class ChapterPageModel : OdkPageModel
{
    protected ChapterPageModel(IRequestCache requestCache)
        : base(requestCache)
    {
    }

    public Chapter Chapter { get; private set; } = null!;
    
    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var chapter = await new ChapterPageContext(RequestCache, HttpContext).GetChapterAsync();
        OdkAssertions.Exists(chapter);

        Chapter = chapter;

        await base.OnPageHandlerExecutionAsync(context, next);
    }
}
