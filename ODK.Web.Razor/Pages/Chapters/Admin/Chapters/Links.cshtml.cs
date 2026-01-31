using ODK.Services.Security;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Chapters;

public class LinksModel : AdminPageModel
{
    public LinksModel()
    {
    }

    public override ChapterAdminSecurable Securable => ChapterAdminSecurable.SocialMedia;

    public void OnGet()
    {
    }
}