using ODK.Core.Chapters;
using ODK.Core.Events;
using ODK.Core.Platforms;
using ODK.Core.Venues;
using ODK.Data.Core.Events;

namespace ODK.Services.Events.ViewModels;

public class EventsAdminPageViewModel
{
    public required Chapter Chapter { get; init; }

    public required IReadOnlyCollection<Event> Events { get; init; }

    public required IReadOnlyCollection<EventInvitesDto> Invites { get; init; }

    public required PlatformType Platform { get; init; }

    public required IReadOnlyCollection<EventResponseSummaryDto> Responses { get; init; }

    public required IReadOnlyCollection<Venue> Venues { get; init; }
}
