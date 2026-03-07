using ODK.Core.Chapters;
using ODK.Services.Security;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Chapters.Conversations;

public class IndexModel : AdminPageModel
{
    public IndexModel()
    {
    }

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