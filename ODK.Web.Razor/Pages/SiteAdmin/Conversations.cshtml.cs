namespace ODK.Web.Razor.Pages.SiteAdmin;

public class ConversationsModel : SiteAdminPageModel
{
    public bool Archived { get; private set; }

    public void OnGet(bool archived = false)
    {
        Archived = archived;
    }
}
