using Microsoft.AspNetCore.Mvc;

namespace ODK.Web.Razor.Pages.Account;

public class SiteConversationModel : OdkSiteAccountPageModel
{
    public Guid ConversationId { get; private set; }

    public void OnGet([FromRoute] Guid id)
    {
        ConversationId = id;
    }
}
