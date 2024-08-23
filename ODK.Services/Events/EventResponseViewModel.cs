using ODK.Core.Events;
using ODK.Core.Venues;

namespace ODK.Services.Events;

public class EventResponseViewModel
{
    public EventResponseViewModel(Event @event, Venue? venue, EventResponseType response,
        bool invited)
    {
        Date = @event.Date;
        EndTime = @event.EndTime;
        EventId = @event.Id;
        EventName = @event.GetDisplayName();
        Invited = invited;
        Response = response;
        Ticketed = @event.Ticketed;
        Time = @event.Time;
        VenueId = venue?.Id;
        VenueName = venue?.Name;
    }

    public DateTime Date { get; }

    public TimeSpan? EndTime { get; }

    public Guid EventId { get; }

    public string EventName { get; }

    public bool Invited { get; }

    public bool Public { get; }

    public EventResponseType Response { get; }

    public bool Ticketed { get; }

    public string? Time { get; }

    public Guid? VenueId { get; }

    public string? VenueName { get; }
}
