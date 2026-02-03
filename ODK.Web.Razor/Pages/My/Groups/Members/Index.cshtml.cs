using ODK.Services.Security;

namespace ODK.Web.Razor.Pages.My.Groups.Members;

public class IndexModel : OdkGroupAdminPageModel
{
    public override ChapterAdminSecurable Securable => ChapterAdminSecurable.Members;

    public void OnGet()
    {
    }
}
