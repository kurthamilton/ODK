using System.Collections.Generic;
using ODK.Core.Events;
using ODK.Core.Members;
using ODK.Core.Settings;
using ODK.Core.Venues;

namespace ODK.Web.Common.Chapters;
public class EventPageViewModel : ChapterViewModel
{
    public required Event Event { get; set; }

    public required IReadOnlyCollection<Member> Members { get; set; }

    public required IReadOnlyCollection<EventResponse> Responses { get; set; }

    public required SiteSettings Settings { get; set; }

    public required Venue Venue { get; set; }
}
