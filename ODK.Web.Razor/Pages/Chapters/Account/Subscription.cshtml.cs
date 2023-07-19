using ODK.Services.Caching;

namespace ODK.Web.Razor.Pages.Chapters.Account
{
    public class SubscriptionModel : ChapterPageModel
    {
        public SubscriptionModel(IRequestCache requestCache) 
            : base(requestCache)
        {
        }

        public void OnGet()
        {
        }
    }
}
