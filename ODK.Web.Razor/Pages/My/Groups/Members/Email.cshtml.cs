using ODK.Services.Security;

namespace ODK.Web.Razor.Pages.My.Groups.Members;

public class EmailModel : OdkGroupAdminPageModel
{
    public override ChapterAdminSecurable Securable => ChapterAdminSecurable.Emails;

    public void OnGet()
    {
    }
}
