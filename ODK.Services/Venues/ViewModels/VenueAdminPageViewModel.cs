using ODK.Core.Chapters;
using ODK.Core.Platforms;
using ODK.Core.Venues;

namespace ODK.Services.Venues.ViewModels;
public class VenueAdminPageViewModel
{
    public required Chapter Chapter { get; init; }

    public required VenueLocation? Location { get; init; }

    public required PlatformType Platform { get; init; }

    public required Venue Venue { get; init; }    
}
