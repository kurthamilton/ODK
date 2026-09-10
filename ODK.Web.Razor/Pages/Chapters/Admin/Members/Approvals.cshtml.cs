using ODK.Services.Security;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Members;

public class ApprovalsModel : AdminPageModel
{
    public override ChapterAdminSecurable Securable => ChapterAdminSecurable.MemberApprovals;

    public void OnGet()
    {
    }
}
