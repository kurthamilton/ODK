using ODK.Services.Security;

namespace ODK.Web.Razor.Pages.My.Groups.Group;

public class IndexModel : OdkGroupAdminPageModel
{
    public override ChapterAdminSecurable Securable => ChapterAdminSecurable.Any;

    public void OnGet()
    {
    }
}
