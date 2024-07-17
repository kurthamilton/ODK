namespace ODK.Core.Events;

public class EventEmail : IDatabaseEntity
{
    public Guid EventId { get; set; }

    public Guid Id { get; set; }

    public DateTime? SentDate { get; set; }
}
