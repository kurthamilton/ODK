using ODK.Services.Security;

namespace ODK.Web.Razor.Pages.My.Groups.Members;

public class MembershipModel : OdkGroupAdminPageModel
{
    public override ChapterAdminSecurable Securable => ChapterAdminSecurable.MembershipSettings;

    public void OnGet()
    {
    }
}
