using ODK.Services.Security;

namespace ODK.Web.Razor.Pages.My.Groups.Group;

public class ThemeModel : OdkGroupAdminPageModel
{
    public override ChapterAdminSecurable Securable => ChapterAdminSecurable.Branding;

    public void OnGet()
    {
    }
}
