using ODK.Core.Events;
using ODK.Core.Venues;

namespace ODK.Services.Venues;
public class VenuesDto
{
    public required IReadOnlyCollection<Event> Events { get; set; }

    public required IReadOnlyCollection<Venue> Venues { get; set; }
}
