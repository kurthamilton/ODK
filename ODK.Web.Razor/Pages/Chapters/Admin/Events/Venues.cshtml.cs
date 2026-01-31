using ODK.Services.Security;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Events;

public class VenuesModel : AdminPageModel
{
    public VenuesModel()
    {
    }

    public override ChapterAdminSecurable Securable => ChapterAdminSecurable.Venues;

    public void OnGet()
    {
    }
}