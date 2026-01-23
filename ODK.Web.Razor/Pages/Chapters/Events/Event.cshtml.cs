using Microsoft.AspNetCore.Mvc;
using ODK.Core.Events;
using ODK.Services.Events;
using ODK.Web.Common.Routes;

namespace ODK.Web.Razor.Pages.Chapters.Events;

public class EventModel : OdkPageModel
{
    private readonly IEventService _eventService;

    public EventModel(IEventService eventService)
    {
        _eventService = eventService;
    }

    public Guid EventId { get; private set; }

    public async Task<IActionResult> OnGet(Guid id, string? rsvp = null)
    {
        EventId = id;

        if (!string.IsNullOrEmpty(rsvp))
        {
            try
            {
                var response = Enum.Parse<EventResponseType>(rsvp, true);
                await _eventService.UpdateMemberResponse(MemberServiceRequest, id, response);
            }
            catch
            {
                // do nothing
            }

            var chapter = await RequestStore.GetChapter();
            var redirectPath = OdkRoutes.Groups.Event(Platform, chapter, id);
            return Redirect(redirectPath);
        }

        return Page();
    }
}