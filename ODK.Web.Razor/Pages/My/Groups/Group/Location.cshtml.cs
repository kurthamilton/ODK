using ODK.Services.Security;

namespace ODK.Web.Razor.Pages.My.Groups.Group;

public class LocationModel : OdkGroupAdminPageModel
{
    public override ChapterAdminSecurable Securable => ChapterAdminSecurable.Location;

    public void OnGet()
    {
    }
}
