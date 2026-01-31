using ODK.Services.Security;

namespace ODK.Web.Razor.Pages.My.Groups.Members.Admins;

public class IndexModel : OdkGroupAdminPageModel
{
    public override ChapterAdminSecurable Securable => ChapterAdminSecurable.AdminMembers;

    public void OnGet()
    {
    }
}
