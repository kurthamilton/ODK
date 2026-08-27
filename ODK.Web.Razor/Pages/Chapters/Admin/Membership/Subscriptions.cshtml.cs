using ODK.Services.Security;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Membership;

public class SubscriptionsModel : AdminPageModel
{
    public SubscriptionsModel()
    {
    }

    public override ChapterAdminSecurable Securable => ChapterAdminSecurable.Subscriptions;

    public void OnGet()
    {
    }
}
