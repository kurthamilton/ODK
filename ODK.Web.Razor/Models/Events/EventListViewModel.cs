using ODK.Services.Events;

namespace ODK.Web.Razor.Models.Events;

public class EventListViewModel
{
    public required string ChapterName { get; init; }

    public required IReadOnlyCollection<EventResponseViewModel> Events { get; init; }
}
