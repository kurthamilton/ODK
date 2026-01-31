using ODK.Services.Security;

namespace ODK.Web.Razor.Pages.My.Groups.Group.Messages;

public class SpamModel : OdkGroupAdminPageModel
{
    public override ChapterAdminSecurable Securable => ChapterAdminSecurable.ContactMessages;

    public void OnGet()
    {
    }
}
