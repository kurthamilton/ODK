using ODK.Core.Venues;

namespace ODK.Data.Core.Venues;

public class ChapterVenueWithLocationDto
{
    public required ChapterVenue ChapterVenue { get; init; }

    public required VenueLocation? Location { get; init; }
}
