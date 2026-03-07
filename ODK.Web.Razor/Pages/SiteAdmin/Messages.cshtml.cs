using ODK.Core.Messages;

namespace ODK.Web.Razor.Pages.SiteAdmin;

public class MessagesModel : SiteAdminPageModel
{
    public MessagesModel()
    {
    }

    public MessageStatus Status { get; private set; }

    public void OnGet(MessageStatus? status = null)
    {
        Status = status ?? MessageStatus.Unreplied;
    }
}