namespace ODK.Core.Events;

public class EventEmail : IDatabaseEntity
{
    public Guid EventId { get; set; }

    public Guid Id { get; set; }

    public string? JobId { get; set; }

    public DateTime? ScheduledUtc { get; set; }

    public DateTime? SentUtc { get; set; }
}