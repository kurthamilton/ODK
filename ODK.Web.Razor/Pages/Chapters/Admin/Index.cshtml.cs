using ODK.Services.Security;

namespace ODK.Web.Razor.Pages.Chapters.Admin;

public class IndexModel : AdminPageModel
{
    public IndexModel()
    {
    }

    public override ChapterAdminSecurable Securable => ChapterAdminSecurable.Any;

    public void OnGet()
    {
    }
}