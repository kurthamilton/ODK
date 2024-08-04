namespace ODK.Web.Razor.Pages.Account;

public class LoginModel : OdkPageModel2
{
    public string? ReturnUrl { get; private set; }

    public void OnGet(string? returnUrl)
    {
        ReturnUrl = returnUrl;
    }
}