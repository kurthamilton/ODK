using Microsoft.AspNetCore.Mvc.Filters;
using ODK.Core;
using ODK.Core.Chapters;
using ODK.Core.Platforms;
using ODK.Web.Common.Extensions;

namespace ODK.Web.Razor.Pages.Chapters;

public abstract class ChapterPageModel2 : OdkPageModel
{
    public string ChapterName { get; private set; } = null!;

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var chapterName = HttpContext.ChapterName();
        OdkAssertions.Exists(chapterName, $"Chapter name missing");

        ChapterName = Chapter.GetFullName(PlatformType.DrunkenKnitwits, chapterName);

        await base.OnPageHandlerExecutionAsync(context, next);
    }
}