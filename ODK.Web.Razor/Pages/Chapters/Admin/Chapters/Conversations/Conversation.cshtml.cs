using ODK.Services.Security;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Chapters.Conversations;

public class ConversationModel : AdminPageModel
{
    public ConversationModel()
    {
    }

    public Guid ConversationId { get; private set; }

    public override ChapterAdminSecurable Securable => ChapterAdminSecurable.Conversations;

    public void OnGet(Guid id)
    {
        ConversationId = id;
    }
}