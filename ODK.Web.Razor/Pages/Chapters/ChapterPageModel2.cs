using System.Net;
using Microsoft.AspNetCore.Mvc.Filters;
using ODK.Web.Common.Chapters;
using ODK.Web.Common.Extensions;

namespace ODK.Web.Razor.Pages.Chapters;

public abstract class ChapterPageModel2<T> : OdkPageModel2<T> where T : ChapterViewModel
{
    protected Guid? MemberId => User.MemberIdOrDefault();

    protected string Name { get; private set; } = null!;

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var chapterName = HttpContext.Request.RouteValues["chapterName"] as string;
        if (chapterName == null)
        {
            HandleMissingChapter();
            return;
        }

        Name = chapterName;

        await base.OnPageHandlerExecutionAsync(context, next);
    }

    protected virtual void HandleMissingChapter()
    {
        Response.StatusCode = (int)HttpStatusCode.NotFound;
    }
}
