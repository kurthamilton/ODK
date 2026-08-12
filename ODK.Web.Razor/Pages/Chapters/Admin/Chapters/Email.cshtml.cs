using ODK.Core.Emails;
using ODK.Services.Security;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Chapters;

public class EmailModel : AdminPageModel
{
    public override ChapterAdminSecurable Securable => ChapterAdminSecurable.Emails;

    public EmailType Type { get; private set; }

    public void OnGet(EmailType type)
    {
        Type = type;
    }
}
