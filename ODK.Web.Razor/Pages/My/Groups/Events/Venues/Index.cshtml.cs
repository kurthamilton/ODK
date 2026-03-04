using ODK.Services.Security;

namespace ODK.Web.Razor.Pages.My.Groups.Events.Venues;

public class IndexModel : OdkGroupAdminPageModel
{
    public bool Archived { get; private set; }

    public override ChapterAdminSecurable Securable => ChapterAdminSecurable.Venues;

    public void OnGet(bool archived = false)
    {
        Archived = archived;
    }
}