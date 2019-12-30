using System.Data;
using ODK.Core.Events;
using ODK.Data.Sql;
using ODK.Data.Sql.Mapping;

namespace ODK.Data.Mapping
{
    public class EventEmailMap : SqlMap<EventEmail>
    {
        public EventEmailMap()
            : base("EventEmails")
        {
            Property(x => x.Id).HasColumnName("EventEmailId").IsIdentity();
            Property(x => x.EventId);
            Property(x => x.EmailProvider);
            Property(x => x.EmailProviderEmailId);
            Property(x => x.SentDate);
        }

        public override EventEmail Read(IDataReader reader)
        {
            return new EventEmail
            (
                id: reader.GetGuid(0),
                eventId: reader.GetGuid(1),
                emailProvider: reader.GetString(2),
                emailProviderEmailId: reader.GetString(3),
                sentDate: reader.GetNullableDateTime(4)
            );
        }
    }
}
