using ODK.Services.Security;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Chapters;

public class ChapterSubscriptionModel : AdminPageModel
{
    public ChapterSubscriptionModel()
    {
    }

    public override ChapterAdminSecurable Securable => ChapterAdminSecurable.SiteSubscription;

    public void OnGet()
    {
    }
}