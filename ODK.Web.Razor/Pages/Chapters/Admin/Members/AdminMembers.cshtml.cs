using ODK.Services.Security;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Members;

public class AdminMembersModel : AdminPageModel
{
    public AdminMembersModel()
    {
    }

    public override ChapterAdminSecurable Securable => ChapterAdminSecurable.AdminMembers;

    public void OnGet()
    {
    }
}