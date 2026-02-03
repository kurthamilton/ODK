using ODK.Services.Security;

namespace ODK.Web.Razor.Pages.My.Groups.Group;

public class PrivacyModel : OdkGroupAdminPageModel
{
    public override ChapterAdminSecurable Securable => ChapterAdminSecurable.PrivacySettings;

    public void OnGet()
    {
    }
}
