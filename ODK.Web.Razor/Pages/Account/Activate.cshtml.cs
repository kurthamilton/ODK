using Microsoft.AspNetCore.Mvc;

namespace ODK.Web.Razor.Pages.Account;

public class ActivateModel : OdkSiteAccountPageModel
{
    public ActivateModel()
    {
    }

    public string Token { get; private set; } = string.Empty;

    public void OnGet([FromQuery] string token)
    {
        Token = token;
    }
}