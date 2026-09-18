using ODK.Core.Events;
using ODK.Core.Venues;

namespace ODK.Data.Core.Events;

public class EventWithVenueDto
{
    public required ChapterVenue ChapterVenue { get; init; }

    public required Event Event { get; init; }
}