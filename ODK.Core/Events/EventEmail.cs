namespace ODK.Core.Events;

public class EventEmail
{
    public EventEmail(Guid id, Guid eventId, DateTime? sentDate)
    {
        EventId = eventId;
        Id = id;
        SentDate = sentDate;
    }

    public Guid EventId { get; }

    public Guid Id { get; }

    public DateTime? SentDate { get; set; }
}
