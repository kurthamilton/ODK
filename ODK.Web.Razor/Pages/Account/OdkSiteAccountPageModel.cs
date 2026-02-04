using Microsoft.AspNetCore.Mvc.Filters;
using ODK.Core.Platforms;
using ODK.Services.Chapters;
using ODK.Web.Razor.Attributes;

namespace ODK.Web.Razor.Pages.Account;

public abstract class OdkSiteAccountPageModel : OdkPageModel
{
    [OdkInject]
    public IChapterService ChapterService { get; set; } = null!;

    public override async Task OnPageHandlerExecutionAsync(
        PageHandlerExecutingContext context,
        PageHandlerExecutionDelegate next)
    {
        if (Platform == PlatformType.Default ||
            CurrentMemberIdOrDefault == null)
        {
            await next();
            return;
        }

        var chapter = await ChapterService.GetDefaultChapter(MemberServiceRequest);

        var redirectPath = chapter == null
            ? "/"
            : OdkRoutes.Account.Index(null);

        context.Result = Redirect(redirectPath);
    }
}