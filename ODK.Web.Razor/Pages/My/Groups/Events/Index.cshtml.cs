using ODK.Services.Security;

namespace ODK.Web.Razor.Pages.My.Groups.Events;

public class IndexModel : OdkGroupAdminPageModel
{
    public override ChapterAdminSecurable Securable => ChapterAdminSecurable.Events;

    public void OnGet()
    {
    }
}
