using ODK.Services.Caching;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Members;

public class SubscriptionsModel : AdminPageModel
{
    public SubscriptionsModel(IRequestCache requestCache)
        : base(requestCache)
    {
    }

    public void OnGet()
    {
    }
}
