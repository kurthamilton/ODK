using ODK.Services.Security;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Members;

public class MemberConversationsModel : AdminPageModel
{
    public MemberConversationsModel()
    {
    }

    public Guid MemberId { get; private set; }

    public override ChapterAdminSecurable Securable => ChapterAdminSecurable.Members;

    public void OnGet(Guid id)
    {
        MemberId = id;
    }
}