using ODK.Services.Security;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Chapters;

public class MessagesSpamModel : AdminPageModel
{
    public MessagesSpamModel()
    {
    }

    public override ChapterAdminSecurable Securable => ChapterAdminSecurable.ContactMessages;

    public void OnGet()
    {
    }
}