namespace ODK.Core.Events;

public class EventResponse
{
    public Guid EventId { get; set; }

    public Guid MemberId { get; set; }

    public EventResponseType Type { get; set; }
}
