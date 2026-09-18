using ODK.Core.Venues;

namespace ODK.Services.Venues.ViewModels;

public class VenueSiteAdminPageViewModel
{
    /// <summary>
    /// How many groups link to the venue. Backfilling changes what every one of them sees, so the page
    /// says how many before it is done rather than after.
    /// </summary>
    public required int ChapterCount { get; init; }

    public required VenueLocation? Location { get; init; }

    public required Venue Venue { get; init; }
}
