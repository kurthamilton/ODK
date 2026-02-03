using ODK.Services.Security;

namespace ODK.Web.Razor.Pages.My.Groups.Events;

public class SettingsModel : OdkGroupAdminPageModel
{
    public override ChapterAdminSecurable Securable => ChapterAdminSecurable.EventSettings;

    public void OnGet()
    {
    }
}
