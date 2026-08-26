using Microsoft.AspNetCore.Mvc;

namespace ODK.Web.Razor.Pages;

public class LoginModel : OdkPageModel
{
    public IActionResult OnGet() => Redirect(OdkRoutes.Account.Login(chapter: null));
}
