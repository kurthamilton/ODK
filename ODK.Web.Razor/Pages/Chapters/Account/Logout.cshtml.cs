using Microsoft.AspNetCore.Mvc;
using ODK.Web.Common.Account;
using ODK.Web.Common.Routes;

namespace ODK.Web.Razor.Pages.Chapters.Account;

public class LogoutModel : ChapterPageModel
{
    private ILoginHandler _loginHandler;

    public LogoutModel(ILoginHandler loginHandler)
    {
        _loginHandler = loginHandler;
    }

    public async Task<IActionResult> OnGet()
    {
        await _loginHandler.Logout();

        var redirectUrl = OdkRoutes.Groups.Group(Platform, Chapter);
        return Redirect(redirectUrl);
    }
}
