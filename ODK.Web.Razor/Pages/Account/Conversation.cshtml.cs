namespace ODK.Web.Razor.Pages.Account;

public class ConversationModel : OdkSiteAccountPageModel
{
    public Guid ConversationId { get; private set; }

    public void OnGet(Guid id)
    {
        ConversationId = id;
    }
}