using Microsoft.AspNetCore.Mvc;
using ODK.Web.Common.Account;

namespace ODK.Web.Razor.Pages.Account;

public class LogoutModel : OdkSiteAccountPageModel
{
    private ILoginHandler _loginHandler;

    public LogoutModel(ILoginHandler loginHandler)
    {
        _loginHandler = loginHandler;
    }

    public async Task<IActionResult> OnGet()
    {
        await _loginHandler.Logout();
        return Redirect("/");
    }
}