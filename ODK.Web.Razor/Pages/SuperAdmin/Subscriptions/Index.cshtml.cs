using ODK.Services.Caching;

namespace ODK.Web.Razor.Pages.SuperAdmin.Subscriptions;

public class IndexModel : SuperAdminPageModel
{
    public IndexModel(IRequestCache requestCache) 
        : base(requestCache)
    {
    }

    public void OnGet()
    {
    }
}
