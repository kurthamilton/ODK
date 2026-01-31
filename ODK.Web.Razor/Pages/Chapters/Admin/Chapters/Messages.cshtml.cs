using ODK.Services.Security;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Chapters;

public class MessagesModel : AdminPageModel
{
    public MessagesModel()
    {
    }

    public override ChapterAdminSecurable Securable => ChapterAdminSecurable.ContactMessages;

    public void OnGet()
    {
    }
}