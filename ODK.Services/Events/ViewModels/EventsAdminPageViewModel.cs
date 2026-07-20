using ODK.Core.Chapters;
using ODK.Core.Utils;
using ODK.Core.Venues;
using ODK.Data.Core.Events;

namespace ODK.Services.Events.ViewModels;

public class EventsAdminPageViewModel
{
    public required Chapter Chapter { get; init; }

    public required PagedResult<EventSummaryDto> Events { get; init; }

    public required EventAdminFilter Filter { get; init; }

    public required IReadOnlyCollection<Venue> Venues { get; init; }
}
