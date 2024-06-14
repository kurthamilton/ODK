using ODK.Core.Chapters;
using ODK.Services.Events;

namespace ODK.Web.Razor.Models.Events;

public class ListEventViewModel
{
    public ListEventViewModel(Chapter chapter, EventResponseViewModel eventResponse)
    {
        Chapter = chapter;
        EventResponse = eventResponse;
    }

    public Chapter Chapter { get; }

    public EventResponseViewModel EventResponse { get; }
}
