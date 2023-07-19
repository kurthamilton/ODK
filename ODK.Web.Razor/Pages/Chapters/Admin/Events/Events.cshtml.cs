using ODK.Services.Caching;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Events
{
    public class EventsModel : AdminPageModel
    {
        public EventsModel(IRequestCache requestCache) 
            : base(requestCache)
        {
        }

        public void OnGet()
        {
        }
    }
}
