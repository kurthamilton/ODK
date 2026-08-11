using ODK.Core.Emails;
using ODK.Services.Security;

namespace ODK.Web.Razor.Pages.My.Groups.Group;

public class EmailModel : OdkGroupAdminPageModel
{
    public override ChapterAdminSecurable Securable => ChapterAdminSecurable.Emails;

    public EmailType Type { get; private set; }

    public void OnGet(EmailType type)
    {
        Type = type;
    }
}
