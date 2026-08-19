namespace ODK.Web.Razor.Pages.Account;

public class SiteConversationsModel : OdkSiteAccountPageModel
{
    public bool Archived { get; private set; }

    public void OnGet(bool archived = false)
    {
        Archived = archived;
    }
}
