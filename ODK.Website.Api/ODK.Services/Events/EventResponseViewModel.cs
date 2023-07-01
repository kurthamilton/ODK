using System;
using ODK.Core.Events;
using ODK.Core.Venues;

namespace ODK.Services.Events
{
    public class EventResponseViewModel
    {
        public EventResponseViewModel(Event @event, Venue venue, EventResponseType response)
        {
            Date = @event.Date;
            EventId = @event.Id;
            EventName = @event.Name;
            Response = response;
            VenueName = venue.Name;
        }

        public DateTime Date { get; }

        public Guid EventId { get; }

        public string EventName { get; }

        public EventResponseType Response { get; }

        public string VenueName { get; }
    }
}
