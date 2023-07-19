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

        protected override IActionResult RedirectToSelf(Guid id)
        {
            return Redirect($"/{Chapter.Name}/Events/{id}");
        }
    }
}
