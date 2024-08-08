using ODK.Services.Caching;

namespace ODK.Web.Razor.Pages.SuperAdmin
{
    public class SubscriptionsModel : SuperAdminPageModel
    {
        public SubscriptionsModel(IRequestCache requestCache) 
            : base(requestCache)
        {
        }

        public void OnGet()
        {
        }
    }
}
