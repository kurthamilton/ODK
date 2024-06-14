using ODK.Services.Caching;

namespace ODK.Web.Razor.Pages.Account;

public class LoginModel : OdkPageModel
{
    public LoginModel(IRequestCache requestCache)
        : base(requestCache)
    {
    }

    public string? ReturnUrl { get; private set; }

    public void OnGet(string? returnUrl)
    {
        ReturnUrl = returnUrl;
    }
}