using System.Data;
using ODK.Core.Events;
using ODK.Data.Sql.Mapping;

namespace ODK.Data.Mapping
{
    public class EventInviteMap : SqlMap<EventInvite>
    {
        public EventInviteMap()
            : base("EventInvites")
        {
            Property(x => x.EventId);
            Property(x => x.MemberId);
            Property(x => x.SentDate);
        }

        public override EventInvite Read(IDataReader reader)
        {
            return new EventInvite
            (
                eventId: reader.GetGuid(0),
                memberId: reader.GetGuid(1),
                sentDate: reader.GetDateTime(2)
            );
        }
    }
}
