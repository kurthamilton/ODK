using ODK.Services.Security;

namespace ODK.Web.Razor.Pages.My.Groups.Events.Venues;

public class IndexModel : OdkGroupAdminPageModel
{
    public override ChapterAdminSecurable Securable => ChapterAdminSecurable.Venues;

    public void OnGet()
    {
    }
}
