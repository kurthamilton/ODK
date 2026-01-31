using ODK.Services.Security;

namespace ODK.Web.Razor.Pages.My.Groups.Members.Properties;

public class IndexModel : OdkGroupAdminPageModel
{
    public override ChapterAdminSecurable Securable => ChapterAdminSecurable.Properties;

    public void OnGet()
    {
    }
}
