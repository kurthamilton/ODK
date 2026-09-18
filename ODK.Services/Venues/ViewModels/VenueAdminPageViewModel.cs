using ODK.Core.Chapters;
using ODK.Core.Platforms;
using ODK.Core.Venues;

namespace ODK.Services.Venues.ViewModels;

public class VenueAdminPageViewModel
{
    public required Chapter Chapter { get; init; }

    /// <summary>
    /// This chapter's link to the venue, which holds the two things it may change: what it calls the
    /// venue and how to find it. <c>Venue</c> is the place itself, shared with every other chapter.
    /// </summary>
    public required ChapterVenue ChapterVenue { get; init; }

    public required VenueLocation? Location { get; init; }

    public required PlatformType Platform { get; init; }
}
