using ODK.Core.Chapters;
using ODK.Core.Events;
using ODK.Core.Venues;

namespace ODK.Web.Razor.Models.Events;

public class EventContentViewModel
{
    public EventContentViewModel(Chapter chapter, Event @event, Venue venue)
    {
        Chapter = chapter;
        Event = @event;
        Venue = venue;
    }

    public Chapter Chapter { get; }

    public Event Event { get; }

    public Venue Venue { get; }
}
