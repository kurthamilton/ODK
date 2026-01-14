namespace ODK.Web.Razor.Pages.Chapters;

public class ConversationModel : OdkPageModel
{
    public Guid ConversationId { get; private set; }

    public void OnGet(Guid id)
    {
        ConversationId = id;
    }
}