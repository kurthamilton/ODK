using Microsoft.AspNetCore.Mvc.Filters;
using ODK.Core;
using ODK.Core.Chapters;
using ODK.Services.Caching;

namespace ODK.Web.Razor.Pages.Chapters;

public abstract class ChapterPageModel : OdkPageModel
{
    private readonly IRequestCache _requestCache;

    protected ChapterPageModel(IRequestCache requestCache)
    {
        _requestCache = requestCache;
    }

    public Chapter Chapter { get; private set; } = null!;
    
    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var chapter = await new ChapterPageContext(_requestCache, HttpContext).GetChapterAsync();
        OdkAssertions.Exists(chapter);

        Chapter = chapter;

        await base.OnPageHandlerExecutionAsync(context, next);
    }
}
