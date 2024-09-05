namespace ODK.Web.Razor.Pages.My.Groups.Group.Conversations;

public class ConversationModel : OdkGroupAdminPageModel
{
    public Guid ConversationId { get; private set; }

    public void OnGet(Guid conversationId)
    {
        ConversationId = conversationId;
    }
}
