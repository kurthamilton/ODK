using System.Data;
using ODK.Core.Venues;
using ODK.Data.Sql;
using ODK.Data.Sql.Mapping;

namespace ODK.Data.Mapping
{
    public class VenueMap : SqlMap<Venue>
    {
        public VenueMap()
            : base("Venues")
        {
            Property(x => x.Id).HasColumnName("VenueId").IsIdentity();
            Property(x => x.ChapterId);
            Property(x => x.Name);
            Property(x => x.Address);
            Property(x => x.MapQuery);
            Property(x => x.Version).IsRowVersion();
        }

        public override Venue Read(IDataReader reader)
        {
            return new Venue
            (
                id: reader.GetGuid(0),
                chapterId: reader.GetGuid(1),
                name: reader.GetString(2),
                address: reader.GetStringOrDefault(3),
                mapQuery: reader.GetStringOrDefault(4),
                version: reader.GetInt64(5)
            );
        }
    }
}
