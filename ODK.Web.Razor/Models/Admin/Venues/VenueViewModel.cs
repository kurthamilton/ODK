using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Core.Venues;

namespace ODK.Web.Razor.Models.Admin.Venues;

public class VenueViewModel
{
    public VenueViewModel(Chapter chapter, Venue venue)
    {
        Chapter = chapter;
        Venue = venue;
    }

    public Chapter Chapter { get; }

    public Venue Venue { get; }
}
