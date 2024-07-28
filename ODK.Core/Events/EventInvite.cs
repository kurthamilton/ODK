namespace ODK.Core.Events;

public class EventInvite
{
    public Guid EventId { get; set; }

    public Guid MemberId { get; set; }

    public DateTime SentUtc { get; set; }
}
