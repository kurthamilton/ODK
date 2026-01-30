namespace ODK.Data.Core.Events;

public class EventResponseSummaryDto
{
    public required Guid EventId { get; init; }

    public required int Yes { get; init; }

    public required int Maybe { get; init; }

    public required int No { get; init; }
}