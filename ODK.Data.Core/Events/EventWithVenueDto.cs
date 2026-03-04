using ODK.Core.Events;
using ODK.Core.Venues;

namespace ODK.Data.Core.Events;

public class EventWithVenueDto
{
    public required Event Event { get; init; }

    public required Venue Venue { get; init; }
}