namespace ODK.Core.Events;

public class EventResponse
{
    public EventResponse(Guid eventId, Guid memberId, EventResponseType responseTypeId)
    {
        EventId = eventId;
        MemberId = memberId;
        ResponseTypeId = responseTypeId;
    }

    public Guid EventId { get; }

    public Guid MemberId { get; }

    public EventResponseType ResponseTypeId { get; }
}
