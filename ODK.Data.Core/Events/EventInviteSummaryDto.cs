namespace ODK.Data.Core.Events;

public class EventInviteSummaryDto
{
    public required Guid EventId { get; init; }

    public required int Sent { get; init; }
}