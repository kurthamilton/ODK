using ODK.Core.Chapters;
using ODK.Services.Events;

namespace ODK.Web.Razor.Models.Events;

public class EventListViewModel
{
    public EventListViewModel(Chapter chapter, IEnumerable<EventResponseViewModel> events)
    {
        Chapter = chapter;
        Events = events.ToArray();
    }

    public Chapter Chapter { get; }

    public IReadOnlyCollection<EventResponseViewModel> Events { get; }
}
