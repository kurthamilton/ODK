using Microsoft.AspNetCore.Mvc;
using ODK.Core.Platforms;

namespace ODK.Web.Razor.Pages;

public class ConversationsModel : OdkPageModel
{
    public bool Archived { get; private set; }

    public IActionResult OnGet(bool archived = false)
    {
        /* Group Squirrel reaches the same conversations through its account area, which this page has no
           equivalent of. Sending it there keeps one canonical URL per platform rather than two that work. */
        if (Platform != PlatformType.DrunkenKnitwits)
        {
            return Redirect(OdkRoutes.Account.SiteConversations(archived));
        }

        Archived = archived;

        return Page();
    }
}
