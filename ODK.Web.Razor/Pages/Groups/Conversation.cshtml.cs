namespace ODK.Web.Razor.Pages.Groups;

public class ConversationModel : OdkPageModel
{
    public Guid ConversationId { get; private set; }

    public void OnGet(Guid id)
    {
        ConversationId = id;
    }
}