namespace ODK.Web.Razor.Pages.Chapters.Admin.Chapters.Conversations;

public class ConversationModel : AdminPageModel
{
    public ConversationModel()
    {
    }

    public Guid ConversationId { get; private set; }

    public void OnGet(Guid id)
    {
        ConversationId = id;
    }
}