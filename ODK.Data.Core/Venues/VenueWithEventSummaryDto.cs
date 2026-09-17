using ODK.Core.Events;

namespace ODK.Data.Core.Venues;

public class VenueWithEventSummaryDto : VenueWithLocationDto
{
    public required int EventCount { get; init; }

    public required Event? LastEvent { get; init; }
}