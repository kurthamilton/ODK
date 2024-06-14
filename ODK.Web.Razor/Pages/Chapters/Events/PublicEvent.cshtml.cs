using Microsoft.AspNetCore.Mvc;
using ODK.Services.Caching;
using ODK.Services.Events;

namespace ODK.Web.Razor.Pages.Chapters.Events;

public class PublicEventModel : EventPageModel
{
    public PublicEventModel(IRequestCache requestCache, IEventService eventService) 
        : base(requestCache, eventService)
    {
    }

    protected override IActionResult RedirectToSelf(Guid id)
    {
        return Redirect($"/{Chapter.Name}/Events/Public/{id}");
    }
}
