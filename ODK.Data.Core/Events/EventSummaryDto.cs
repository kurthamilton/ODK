using ODK.Core.Events;
using ODK.Core.Venues;

namespace ODK.Data.Core.Events;

public class EventSummaryDto
{
    public required Event Event { get; init; }

    public required EventEmail? Email { get; init; }

    public required EventInviteSummaryDto Invites { get; init; }

    public required EventResponseSummaryDto Responses { get; init; }

    public required Venue Venue { get; init; }
}
