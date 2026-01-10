using Microsoft.AspNetCore.Mvc.Filters;
using ODK.Core;
using ODK.Web.Common.Extensions;
using ODK.Web.Razor.Pages.Chapters;

namespace ODK.Web.Razor.Pages.Groups;

public abstract class OdkGroupPageModel : OdkPageModel
{
    public string Slug { get; private set; } = null!;

    public override async Task OnPageHandlerExecutionAsync(
        PageHandlerExecutingContext context,
        PageHandlerExecutionDelegate next)
    {
        var slug = HttpContext.ChapterSlug();
        OdkAssertions.Exists(slug, "Chapter slug missing");

        Slug = slug;

        await base.OnPageHandlerExecutionAsync(context, next);
    }
}