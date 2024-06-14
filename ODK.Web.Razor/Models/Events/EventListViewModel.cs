using ODK.Core.Chapters;
using ODK.Services.Events;

namespace ODK.Web.Razor.Models.Events;

public class EventListViewModel
{
    public EventListViewModel(Chapter chapter, IEnumerable<EventResponseViewModel> eventResponses)
    {
        Chapter = chapter;
        EventResponses = eventResponses.ToArray();
    }

    public Chapter Chapter { get; }

    public IReadOnlyCollection<EventResponseViewModel> EventResponses { get; }
}
