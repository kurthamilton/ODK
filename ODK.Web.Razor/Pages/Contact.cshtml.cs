using Microsoft.AspNetCore.Mvc;

namespace ODK.Web.Razor.Pages;

public class ContactModel : OdkPageModel
{
    /// <summary>
    /// A signed-in member is sent to their conversations instead, which is what the group contact page
    /// already does. The link in the site's chrome points there directly; this redirect is for anything
    /// still holding the old URL.
    /// </summary>
    public IActionResult OnGet()
    {
        if (CurrentMemberOrDefault != null)
        {
            return Redirect(OdkRoutes.Account.SiteConversations());
        }

        return Page();
    }
}
