using ODK.Services.Security;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Chapters;

public class PagesModel : AdminPageModel
{
    public PagesModel()
    {
    }

    public override ChapterAdminSecurable Securable => ChapterAdminSecurable.Pages;

    public void OnGet()
    {
    }
}