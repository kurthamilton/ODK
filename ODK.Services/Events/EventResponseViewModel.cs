using ODK.Core.Events;
using ODK.Core.Venues;

namespace ODK.Services.Events;

public class EventResponseViewModel
{
    public EventResponseViewModel(Event @event, Venue venue, EventResponseType response,
        bool invited, bool @public)
    {
        Date = @event.Date;
        EventId = @event.Id;
        EventName = @event.GetDisplayName();
        Invited = invited;
        Public = @public;
        Response = response;
        VenueId = venue.Id;
        VenueName = venue.Name;
    }

    public DateTime Date { get; }

    public Guid EventId { get; }

    public string EventName { get; }

    public bool Invited { get; }

    public bool Public { get; }

    public EventResponseType Response { get; }

    public Guid VenueId { get; }

    public string VenueName { get; }
}
