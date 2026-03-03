namespace ODK.Web.Razor.Pages.Chapters.Account;

public class ConversationsModel : OdkPageModel
{
    public bool Archived { get; private set; }

    public void OnGet(bool archived = false)
    {
        Archived = archived;
    }
}