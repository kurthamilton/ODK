using ODK.Services.Security;

namespace ODK.Web.Razor.Pages.My.Groups.Membership.Subscriptions;

public class IndexModel : OdkGroupAdminPageModel
{
    public override ChapterAdminSecurable Securable => ChapterAdminSecurable.Subscriptions;

    public void OnGet()
    {
    }
}
