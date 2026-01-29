namespace ODK.Web.Razor.Pages.Chapters.Account;

public class LoginModel : OdkPageModel
{
    public string? ReturnUrl { get; private set; }

    public async Task OnGet(string? returnUrl)
    {
        if (CurrentMemberIdOrDefault != null)
        {
            Response.Redirect(returnUrl ?? OdkRoutes.Groups.Group(Chapter));
        }

        ReturnUrl = returnUrl;
    }
}