using ODK.Core.Events;
using ODK.Core.Members;

namespace ODK.Services.Events;
public class EventResponsesDto
{
    public required IReadOnlyCollection<Member> Members { get; set; }

    public required IReadOnlyCollection<EventResponse> Responses { get; set; }
}
