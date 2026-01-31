using ODK.Services.Security;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Events;

public class SettingsModel : AdminPageModel
{
    public SettingsModel()
    {
    }

    public override ChapterAdminSecurable Securable => ChapterAdminSecurable.EventSettings;

    public void OnGet()
    {
    }
}