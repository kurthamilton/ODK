using ODK.Services.Security;

namespace ODK.Web.Razor.Pages.My.Groups.Group.Conversations;

public class IndexModel : OdkGroupAdminPageModel
{
    public override ChapterAdminSecurable Securable => ChapterAdminSecurable.Conversations;

    public void OnGet()
    {
    }
}
