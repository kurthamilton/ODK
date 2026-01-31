using ODK.Services.Security;

namespace ODK.Web.Razor.Pages.My.Groups.Group.Messages;

public class MessageModel : OdkGroupAdminPageModel
{
    public Guid MessageId { get; private set; }

    public override ChapterAdminSecurable Securable => ChapterAdminSecurable.ContactMessages;

    public void OnGet(Guid messageId)
    {
        MessageId = messageId;
    }
}
