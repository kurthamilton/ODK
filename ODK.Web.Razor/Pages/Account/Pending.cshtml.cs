using Microsoft.AspNetCore.Mvc;
using ODK.Core.Members;

namespace ODK.Web.Razor.Pages.Account;

public class PendingModel : OdkSiteAccountPageModel
{
    /// <summary>What the sign-up was for, so the page can name what happens after the activation link.</summary>
    public SignUpIntentType? Intent { get; private set; }

    public void OnGet([FromQuery] SignUpIntentType? intent)
    {
        Intent = intent;
    }
}