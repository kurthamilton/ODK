using Microsoft.AspNetCore.Mvc;
using ODK.Core.Platforms;
using ODK.Services.Chapters;
using ODK.Web.Common.Account;

namespace ODK.Web.Razor.Pages.Account;

public class LogoutModel : OdkPageModel
{
    private readonly IChapterService _chapterService;
    private readonly ILoginHandler _loginHandler;

    public LogoutModel(IChapterService chapterService, ILoginHandler loginHandler)
    {
        _chapterService = chapterService;
        _loginHandler = loginHandler;
    }

    public async Task<IActionResult> OnGet()
    {
        // The landing page depends on the current member, so resolve it before signing out.
        var redirectPath = await GetRedirectPath();

        await _loginHandler.Logout();

        return Redirect(redirectPath);
    }

    private async Task<string> GetRedirectPath()
    {
        // Group Squirrel is a site-level platform, so its members belong on the site home. Drunken
        // Knitwits is a group-level platform, so a member of a single group belongs on that group's page.
        if (Platform != PlatformType.DrunkenKnitwits)
        {
            return "/";
        }

        var chapter = await _chapterService.GetSoleChapter(MemberServiceRequest);
        return chapter != null
            ? OdkRoutes.Groups.Group(chapter)
            : "/";
    }
}