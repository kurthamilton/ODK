using System;
using ODK.Core.Events;
using ODK.Core.Venues;

namespace ODK.Services.Events
{
    public class EventResponseViewModel
    {
        public EventResponseViewModel(Event @event, Venue venue, EventResponseType response,
            bool invited)
        {
            Date = @event.Date;
            EventId = @event.Id;
            EventName = @event.Name;
            Invited = invited;
            Response = response;
            VenueId = venue.Id;
            VenueName = venue.Name;
        }

        public DateTime Date { get; }

        public Guid EventId { get; }

        public string EventName { get; }

        public bool Invited { get; }

        public EventResponseType Response { get; }

        public Guid VenueId { get; }

        public string VenueName { get; }
    }
}
