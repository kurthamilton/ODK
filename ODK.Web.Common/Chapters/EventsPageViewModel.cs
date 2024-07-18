using System.Collections.Generic;
using ODK.Services.Events;

namespace ODK.Web.Common.Chapters;
public class EventsPageViewModel : ChapterViewModel
{
    public required IReadOnlyCollection<EventResponseViewModel> Events { get; set; }
}
