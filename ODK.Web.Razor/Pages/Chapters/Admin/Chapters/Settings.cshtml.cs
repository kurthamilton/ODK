using ODK.Services.Security;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Chapters;

public class SettingsModel : AdminPageModel
{
    public override ChapterAdminSecurable Securable => ChapterAdminSecurable.GroupSettings;

    public void OnGet()
    {
    }
}
