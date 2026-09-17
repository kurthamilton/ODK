using ODK.Core.Venues;

namespace ODK.Data.Core.Venues;

/// <summary>
/// A venue and where it is. The two are read together when deciding whether a place has changed, because
/// the comparison is over the venue's name and its location's coordinates.
/// </summary>
public class VenueWithLocationDto
{
    public required VenueLocation? Location { get; init; }

    public required Venue Venue { get; init; }
}
