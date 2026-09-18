using ODK.Core.Venues;

namespace ODK.Data.Core.Venues;

/// <summary>
/// A venue and how many chapters link to it. The count is the whole point of the site admin list: it says
/// whether venues are being shared, and a zero says the venue is an orphan nothing can now reach.
/// </summary>
public class VenueWithChapterCountDto
{
    public required int ChapterCount { get; init; }

    public required VenueLocation? Location { get; init; }

    public required Venue Venue { get; init; }
}
