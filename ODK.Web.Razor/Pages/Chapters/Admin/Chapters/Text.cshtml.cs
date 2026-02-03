using ODK.Services.Security;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Chapters;

public class TextModel : AdminPageModel
{
    public TextModel()
    {
    }

    public override ChapterAdminSecurable Securable => ChapterAdminSecurable.Texts;

    public void OnGet()
    {
    }
}