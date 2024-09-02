namespace ODK.Web.Razor.Pages.My.Groups.Group.Messages;

public class MessageModel : OdkGroupAdminPageModel
{
    public Guid MessageId { get; private set; }

    public void OnGet(Guid messageId)
    {
        MessageId = messageId;
    }
}
