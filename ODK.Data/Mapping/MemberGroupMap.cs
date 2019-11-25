using System.Data;
using ODK.Core.Members;
using ODK.Data.Sql.Mapping;

namespace ODK.Data.Mapping
{
    public class MemberGroupMap : SqlMap<MemberGroup>
    {
        public MemberGroupMap()
            : base("MemberGroups")
        {
            Property(x => x.Id).HasColumnName("MemberGroupId").IsIdentity();
            Property(x => x.ChapterId);
            Property(x => x.Name);
        }

        public override MemberGroup Read(IDataReader reader)
        {
            return new MemberGroup
            (
                id: reader.GetGuid(0),
                chapterId: reader.GetGuid(1),
                name: reader.GetString(2)
            );
        }
    }
}
