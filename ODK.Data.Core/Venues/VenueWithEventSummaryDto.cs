using ODK.Core.Events;
using ODK.Core.Venues;

namespace ODK.Data.Core.Venues;

public class VenueWithEventSummaryDto
{
    public required int EventCount { get; init; }

    public required Event? LastEvent { get; init; }

    public required Venue Venue { get; init; }
}