using ODK.Core.Messages;
using ODK.Services.Security;

namespace ODK.Web.Razor.Pages.My.Groups.Group.Messages;

public class IndexModel : OdkGroupAdminPageModel
{
    public override ChapterAdminSecurable Securable => ChapterAdminSecurable.ContactMessages;

    public MessageStatus Status { get; private set; }

    public void OnGet(MessageStatus? status = null)
    {
        Status = status ?? MessageStatus.Unreplied;
    }
}