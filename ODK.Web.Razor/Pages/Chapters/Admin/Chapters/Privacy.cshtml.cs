using ODK.Services.Security;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Chapters;

public class PrivacyModel : AdminPageModel
{
    public PrivacyModel()
    {
    }

    public override ChapterAdminSecurable Securable => ChapterAdminSecurable.PrivacySettings;

    public void OnGet()
    {
    }
}