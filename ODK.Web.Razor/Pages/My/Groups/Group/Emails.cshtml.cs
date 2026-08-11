using ODK.Services.Security;

namespace ODK.Web.Razor.Pages.My.Groups.Group;

public class EmailsModel : OdkGroupAdminPageModel
{
    public override ChapterAdminSecurable Securable => ChapterAdminSecurable.Emails;

    public void OnGet()
    {
    }
}
