using ODK.Services.Security;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Chapters.Conversations;

public class RepliedModel : AdminPageModel
{
    public RepliedModel()
    {
    }

    public override ChapterAdminSecurable Securable => ChapterAdminSecurable.Conversations;

    public void OnGet()
    {
    }
}