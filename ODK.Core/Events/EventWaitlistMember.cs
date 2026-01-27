namespace ODK.Core.Events;

public class EventWaitlistMember : IDatabaseEntity
{
    public DateTime CreatedUtc { get; set; }

    public Guid EventId { get; set; }

    public Guid Id { get; set; }

    public Guid MemberId { get; set; }
}