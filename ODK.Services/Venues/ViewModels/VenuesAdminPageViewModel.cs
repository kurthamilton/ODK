using ODK.Core.Chapters;
using ODK.Data.Core.Venues;

namespace ODK.Services.Venues.ViewModels;

public class VenuesAdminPageViewModel
{
    public required int ActiveVenueCount { get; init; }

    public required bool Archived { get; init; }

    public required int ArchivedVenueCount { get; init; }

    public required Chapter Chapter { get; init; }

    public required IReadOnlyCollection<VenueWithEventSummaryDto> Venues { get; init; }
}