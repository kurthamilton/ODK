using Microsoft.AspNetCore.Mvc;
using ODK.Services.Caching;
using ODK.Services.Events;

namespace ODK.Web.Razor.Pages.Chapters.Events
{
    public class EventModel : EventPageModel
    {
        public EventModel(IRequestCache requestCache, IEventService eventService)
            : base(requestCache, eventService)
        {
        }

        protected override IActionResult RedirectToSelf()
        {
            return Redirect($"/{Chapter.Name}/Events/{Event.Id}");
        }
    }
}
