using ODK.Services.Security;

namespace ODK.Web.Razor.Pages.My.Groups.Members;

public class ApprovalsModel : OdkGroupAdminPageModel
{
    public override ChapterAdminSecurable Securable => ChapterAdminSecurable.MemberApprovals;

    public void OnGet()
    {
    }
}
