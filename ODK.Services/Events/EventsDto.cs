using ODK.Core.Events;
using ODK.Core.Venues;

namespace ODK.Services.Events;
public class EventsDto
{
    public required IReadOnlyCollection<Event> Events { get; set; }

    public required IReadOnlyCollection<EventInvitesDto> Invites { get; set; }

    public required IReadOnlyCollection<EventResponseSummaryDto> Responses { get; set; }

    public required IReadOnlyCollection<Venue> Venues { get; set; }
}
