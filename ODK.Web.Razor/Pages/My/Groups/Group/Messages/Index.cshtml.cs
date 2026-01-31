using ODK.Services.Security;

namespace ODK.Web.Razor.Pages.My.Groups.Group.Messages;

public class IndexModel : OdkGroupAdminPageModel
{
    public override ChapterAdminSecurable Securable => ChapterAdminSecurable.ContactMessages;

    public void OnGet()
    {
    }
}