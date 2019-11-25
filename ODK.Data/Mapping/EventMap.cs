using System.Data;
using ODK.Core.Events;
using ODK.Data.Sql;
using ODK.Data.Sql.Mapping;

namespace ODK.Data.Mapping
{
    public class EventMap : SqlMap<Event>
    {
        public EventMap()
            : base("Events")
        {
            Property(x => x.Id).HasColumnName("EventId").IsIdentity();
            Property(x => x.ChapterId);
            Property(x => x.Name);
            Property(x => x.Date);
            Property(x => x.Location);
            Property(x => x.Time);
            Property(x => x.ImageUrl);
            Property(x => x.Address);
            Property(x => x.MapQuery);
            Property(x => x.Description);
            Property(x => x.IsPublic);
        }

        public override Event Read(IDataReader reader)
        {
            return new Event
            (
                id: reader.GetGuid(0),
                chapterId: reader.GetGuid(1),
                name: reader.GetString(2),
                date: reader.GetDateTime(3),
                location: reader.GetString(4),
                time: reader.GetStringOrDefault(5),
                imageUrl: reader.GetStringOrDefault(6),
                address: reader.GetStringOrDefault(7),
                mapQuery: reader.GetStringOrDefault(8),
                description: reader.GetStringOrDefault(9),
                isPublic: reader.GetBoolean(10)
            );
        }
    }
}
