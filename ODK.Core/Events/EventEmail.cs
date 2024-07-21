namespace ODK.Core.Events;

public class EventEmail : IDatabaseEntity
{
    public Guid EventId { get; set; }

    public Guid Id { get; set; }

    /// <summary>
    /// Uses local time for now while chapter times zones are not part of the system
    /// </summary>
    public DateTime? ScheduledDate { get; set; }

    public DateTime? SentDate { get; set; }
}
