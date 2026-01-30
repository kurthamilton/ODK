using ODK.Core.Chapters;
using ODK.Data.Core.Events;

namespace ODK.Services.Events.ViewModels;

public class EventsAdminPageViewModel
{
    public required Chapter Chapter { get; init; }

    public required IReadOnlyCollection<EventSummaryDto> Events { get; init; }
}
