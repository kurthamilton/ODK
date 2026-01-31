using ODK.Services.Security;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Members;

public class PropertiesModel : AdminPageModel
{
    public PropertiesModel()
    {
    }

    public override ChapterAdminSecurable Securable => ChapterAdminSecurable.Properties;

    public void OnGet()
    {
    }
}