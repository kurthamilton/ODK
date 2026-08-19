using Microsoft.AspNetCore.Mvc;
using ODK.Core.Platforms;

namespace ODK.Web.Razor.Pages;

public class ConversationModel : OdkPageModel
{
    public Guid ConversationId { get; private set; }

    public IActionResult OnGet([FromRoute] Guid id)
    {
        // As with the list: one canonical URL per platform.
        if (Platform != PlatformType.DrunkenKnitwits)
        {
            return Redirect(OdkRoutes.Account.SiteConversation(id));
        }

        ConversationId = id;

        return Page();
    }
}
