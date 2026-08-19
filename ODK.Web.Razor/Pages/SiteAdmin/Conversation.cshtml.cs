using Microsoft.AspNetCore.Mvc;

namespace ODK.Web.Razor.Pages.SiteAdmin;

public class ConversationModel : SiteAdminPageModel
{
    public Guid ConversationId { get; private set; }

    public void OnGet([FromRoute] Guid id)
    {
        ConversationId = id;
    }
}
