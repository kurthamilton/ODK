using Microsoft.AspNetCore.Mvc.Filters;
using ODK.Core;
using ODK.Core.Chapters;
using ODK.Web.Common.Extensions;

namespace ODK.Web.Razor.Pages.Groups;

public abstract class OdkGroupPageModel : OdkPageModel
{
    public Chapter Chapter { get; private set; } = null!;

    public string Slug { get; private set; } = null!;

    public override async Task OnPageHandlerExecutionAsync(
        PageHandlerExecutingContext context,
        PageHandlerExecutionDelegate next)
    {
        var slug = HttpContext.ChapterSlug();
        OdkAssertions.Exists(slug, "Chapter slug missing");

        Slug = slug;
        Chapter = await GetChapter();

        await base.OnPageHandlerExecutionAsync(context, next);
    }
}