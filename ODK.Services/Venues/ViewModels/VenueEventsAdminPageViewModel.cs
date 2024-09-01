using ODK.Core.Chapters;
using ODK.Core.Events;
using ODK.Core.Platforms;
using ODK.Core.Venues;

namespace ODK.Services.Venues.ViewModels;

public class VenueEventsAdminPageViewModel
{
    public required Chapter Chapter { get; init; }

    public required IReadOnlyCollection<Event> Events { get; init; }

    public required PlatformType Platform { get; init; }

    public required Venue Venue { get; init; }
}
