using System.Data;
using ODK.Core.Events;
using ODK.Data.Sql;
using ODK.Data.Sql.Mapping;

namespace ODK.Data.Mapping;

public class EventMap : SqlMap<Event>
{
    public EventMap()
        : base("Events")
    {
        Property(x => x.Id).HasColumnName("EventId").IsIdentity();
        Property(x => x.ChapterId);
        Property(x => x.CreatedBy);
        Property(x => x.Name);
        Property(x => x.Date);
        Property(x => x.VenueId);
        Property(x => x.Time);
        Property(x => x.ImageUrl);
        Property(x => x.Description);
        Property(x => x.IsPublic);
    }

    public override Event Read(IDataReader reader)
    {
        return new Event
        (
            id: reader.GetGuid(0),
            chapterId: reader.GetGuid(1),
            createdBy: reader.GetString(2),
            name: reader.GetString(3),
            date: reader.GetDateTime(4),
            venueId: reader.GetGuid(5),
            time: reader.GetStringOrDefault(6),
            imageUrl: reader.GetStringOrDefault(7),
            description: reader.GetStringOrDefault(8),
            isPublic: reader.GetBoolean(9)
        );
    }
}
