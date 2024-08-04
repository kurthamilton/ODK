using ODK.Core.Members;

namespace ODK.Core.Events;

public class EventHost : IDatabaseEntity
{    
    public Guid EventId { get; set; }

    public Guid Id { get; set; }

    public Member Member { get; set; } = null!;

    public Guid MemberId { get; set; }
}
