using ODK.Services.Security;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Venues;

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