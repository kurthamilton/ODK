using ODK.Services.Security;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Members;

public class MemberImageModel : AdminPageModel
{
    public MemberImageModel()
    {
    }

    public Guid MemberId { get; private set; }

    public override ChapterAdminSecurable Securable => ChapterAdminSecurable.MemberAdmin;

    public void OnGet(Guid id)
    {
        MemberId = id;
    }
}