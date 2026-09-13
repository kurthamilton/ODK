using ODK.Services.Security;

namespace ODK.Web.Razor.Pages.My.Groups.Group;

public class MovedModel : OdkGroupAdminPageModel
{
    public override ChapterAdminSecurable Securable => ChapterAdminSecurable.MovedPage;

    public void OnGet()
    {
    }
}
