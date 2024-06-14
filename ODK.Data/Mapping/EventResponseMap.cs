using System.Data;
using ODK.Core.Events;
using ODK.Data.Sql.Mapping;

namespace ODK.Data.Mapping;

public class EventResponseMap : SqlMap<EventResponse>
{
    public EventResponseMap()
        : base("EventResponses")
    {
        Property(x => x.EventId);
        Property(x => x.MemberId);
        Property(x => x.ResponseTypeId);
    }

    public override EventResponse Read(IDataReader reader)
    {
        return new EventResponse
        (
            eventId: reader.GetGuid(0),
            memberId: reader.GetGuid(1),
            responseTypeId: (EventResponseType)reader.GetInt32(2)
        );
    }
}
