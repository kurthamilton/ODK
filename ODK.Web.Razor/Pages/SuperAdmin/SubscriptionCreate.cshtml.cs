using ODK.Services.Caching;

namespace ODK.Web.Razor.Pages.SuperAdmin
{
    public class SubscriptionCreateModel : SuperAdminPageModel
    {
        public SubscriptionCreateModel(IRequestCache requestCache) 
            : base(requestCache)
        {
        }

        public void OnGet()
        {
        }
    }
}
