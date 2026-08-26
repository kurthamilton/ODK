using Microsoft.AspNetCore.Mvc;

namespace ODK.Web.Razor.Pages.Chapters.Account;

public class LoginModel : OdkPageModel
{
    public string? ReturnUrl { get; private set; }

    public IActionResult OnGet(string? returnUrl)
    {
        if (CurrentMemberOrDefault != null)
        {
            return Redirect(Url.IsLocalUrl(returnUrl) ? returnUrl : OdkRoutes.Groups.Group(Chapter));
        }

        ReturnUrl = returnUrl;
        return Page();
    }
}
