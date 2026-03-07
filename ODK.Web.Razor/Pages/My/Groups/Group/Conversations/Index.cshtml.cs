using ODK.Core.Chapters;
using ODK.Services.Security;

namespace ODK.Web.Razor.Pages.My.Groups.Group.Conversations;

public class IndexModel : OdkGroupAdminPageModel
{
    public override ChapterAdminSecurable Securable => ChapterAdminSecurable.Conversations;

    public ChapterConversationStatus Status { get; private set; }

    public void OnGet(ChapterConversationStatus? status = null)
    {
        if (status == null || status == ChapterConversationStatus.None || !Enum.IsDefined(status.Value))
        {
            status = ChapterConversationStatus.Unread;
        }

        Status = status.Value;
    }
}
