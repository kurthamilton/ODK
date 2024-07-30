using ODK.Core.Members;
using ODK.Core.Venues;

namespace ODK.Services.Events;

public class EventDto
{
    public required EventCommentsDto Comments { get; set; }

    public required IReadOnlyCollection<Member> Hosts { get; set; }

    public required Venue Venue { get; set; }
}
