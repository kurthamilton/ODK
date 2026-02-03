using ODK.Services.Security;

namespace ODK.Web.Razor.Pages.My.Groups.Group.Conversations;

public class RepliedModel : OdkGroupAdminPageModel
{
    public override ChapterAdminSecurable Securable => ChapterAdminSecurable.ContactMessages;

    public void OnGet()
    {
    }
}
