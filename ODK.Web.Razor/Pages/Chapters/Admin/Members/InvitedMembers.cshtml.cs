using ODK.Services.Security;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Members;

public class InvitedMembersModel : AdminPageModel
{
    public override ChapterAdminSecurable Securable => ChapterAdminSecurable.MemberImport;

    public void OnGet()
    {
    }
}
