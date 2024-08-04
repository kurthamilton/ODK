using Microsoft.AspNetCore.Mvc;
using ODK.Core.Events;
using ODK.Services.Events;

namespace ODK.Web.Razor.Pages.Chapters.Events;

public class EventModel : ChapterPageModel2
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
                await _eventService.UpdateMemberResponse(MemberId, id, response);
            }
            catch
            {
                // do nothing
            }

            return Redirect($"/{ChapterName}/Events/{id}");
        }

        return Page();
    }
}
