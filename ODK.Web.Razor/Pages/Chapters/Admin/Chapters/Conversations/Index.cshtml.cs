using ODK.Services.Security;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Chapters.Conversations;

public class IndexModel : AdminPageModel
{
    public IndexModel()
    {
    }

    public override ChapterAdminSecurable Securable => ChapterAdminSecurable.Conversations;

    public void OnGet()
    {
    }
}