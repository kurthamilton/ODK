using ODK.Services.Caching;
using ODK.Services.Events;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Events
{
    public class EventTicketsModel : EventAdminPageModel
    {
        public EventTicketsModel(IRequestCache requestCache, IEventAdminService eventAdminService)
            : base(requestCache, eventAdminService)
        {
        }

        public void OnGet()
        {
        }
    }
}
