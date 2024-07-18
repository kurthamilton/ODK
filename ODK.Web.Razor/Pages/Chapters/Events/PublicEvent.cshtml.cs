using Microsoft.AspNetCore.Mvc;
using ODK.Services.Events;
using ODK.Web.Common.Chapters;

namespace ODK.Web.Razor.Pages.Chapters.Events;

public class PublicEventModel : EventPageModel
{
    public PublicEventModel(IChapterWebService chapterWebService, IEventService eventService) 
        : base(chapterWebService, eventService)
    {
    }

    protected override IActionResult RedirectToSelf(Guid id)
    {
        return Redirect($"/{Name}/Events/Public/{id}");
    }
}
