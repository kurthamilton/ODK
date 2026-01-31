using ODK.Services.Security;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Chapters;

public class EmailsModel : AdminPageModel
{
    public EmailsModel()
    {
    }

    public override ChapterAdminSecurable Securable => ChapterAdminSecurable.Any;

    public void OnGet()
    {
    }
}