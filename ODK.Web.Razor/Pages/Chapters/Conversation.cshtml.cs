namespace ODK.Web.Razor.Pages.Chapters;

public class ConversationModel : ChapterPageModel
{
    public Guid ConversationId { get; private set; }

    public void OnGet(Guid id)
    {
        ConversationId = id;
    }
}
