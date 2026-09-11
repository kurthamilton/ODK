using ODK.Services.Security;

namespace ODK.Web.Razor.Pages.My.Groups.Group;

public class SettingsModel : OdkGroupAdminPageModel
{
    public override ChapterAdminSecurable Securable => ChapterAdminSecurable.GroupSettings;

    public void OnGet()
    {
    }
}
