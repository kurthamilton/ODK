using ODK.Services.Security;

namespace ODK.Web.Razor.Pages.My.Groups.Group;

public class TopicsModel : OdkGroupAdminPageModel
{
    public override ChapterAdminSecurable Securable => ChapterAdminSecurable.Topics;

    public void OnGet()
    {
    }
}
