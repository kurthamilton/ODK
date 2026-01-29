using Microsoft.AspNetCore.Mvc;
using ODK.Web.Common.Account;

namespace ODK.Web.Razor.Pages.Chapters.Account;

public class LogoutModel : OdkPageModel
{
    private ILoginHandler _loginHandler;

    public LogoutModel(ILoginHandler loginHandler)
    {
        _loginHandler = loginHandler;
    }

    public async Task<IActionResult> OnGet()
    {
        await _loginHandler.Logout();

        var redirectUrl = OdkRoutes.Groups.Group(Chapter);
        return Redirect(redirectUrl);
    }
}