namespace ODK.Core.Events;

public class EventHost : IDatabaseEntity
{    
    public Guid EventId { get; set; }

    public Guid Id { get; set; }

    public Guid MemberId { get; set; }
}
