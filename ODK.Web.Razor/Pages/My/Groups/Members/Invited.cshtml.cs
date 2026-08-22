using ODK.Services.Security;

namespace ODK.Web.Razor.Pages.My.Groups.Members;

public class InvitedModel : OdkGroupAdminPageModel
{
    public override ChapterAdminSecurable Securable => ChapterAdminSecurable.MemberImport;

    public void OnGet()
    {
    }
}
