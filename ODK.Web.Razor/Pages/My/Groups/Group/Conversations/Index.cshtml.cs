using ODK.Core.Messages;
using ODK.Services.Security;

namespace ODK.Web.Razor.Pages.My.Groups.Group.Conversations;

public class IndexModel : OdkGroupAdminPageModel
{
    public override ChapterAdminSecurable Securable => ChapterAdminSecurable.Conversations;

    public MessageStatus Status { get; private set; }

    public void OnGet(MessageStatus? status = null)
    {
        if (status == null || status == MessageStatus.None || !Enum.IsDefined(status.Value))
        {
            status = MessageStatus.Unread;
        }

        Status = status.Value;
    }
}
