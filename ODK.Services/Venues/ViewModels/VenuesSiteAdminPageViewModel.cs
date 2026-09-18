using ODK.Data.Core.Venues;

namespace ODK.Services.Venues.ViewModels;

public class VenuesSiteAdminPageViewModel
{
    public required IReadOnlyCollection<VenueWithChapterCountDto> Venues { get; init; }
}
