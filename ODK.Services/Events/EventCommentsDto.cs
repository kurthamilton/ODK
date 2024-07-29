using ODK.Core.Events;
using ODK.Core.Members;

namespace ODK.Services.Events;

public class EventCommentsDto
{
    public required IReadOnlyCollection<EventComment>? Comments { get; set; }

    public required IReadOnlyCollection<Member>? Members { get; set; }
}
