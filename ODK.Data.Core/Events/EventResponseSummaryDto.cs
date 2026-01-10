namespace ODK.Data.Core.Events;

public class EventResponseSummaryDto
{
    public Guid EventId { get; set; }

    public int Yes { get; set; }

    public int Maybe { get; set; }

    public int No { get; set; }
}