using ODK.Core.Chapters;
using ODK.Core.Venues;

namespace ODK.Web.Razor.Models.Admin.Venues;

public class VenueViewModel
{
    public VenueViewModel(Chapter chapter, Venue venue, VenueLocation? location)
    {
        Chapter = chapter;
        Location = location;
        Venue = venue;
    }

    public Chapter Chapter { get; }

    public VenueLocation? Location { get; }

    public Venue Venue { get; }
}
