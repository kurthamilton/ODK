using System.Data;
using ODK.Core.Members;
using ODK.Data.Sql;
using ODK.Data.Sql.Mapping;

namespace ODK.Data.Mapping;

public class MemberPropertyMap : SqlMap<MemberProperty>
{
    public MemberPropertyMap()
        : base("MemberProperties")
    {
        Property(x => x.Id).HasColumnName("MemberPropertyId").IsIdentity();
        Property(x => x.MemberId);
        Property(x => x.ChapterPropertyId);
        Property(x => x.Value);
    }

    public override MemberProperty Read(IDataReader reader)
    {
        return new MemberProperty
        (
            id: reader.GetGuid(0),
            memberId: reader.GetGuid(1),
            chapterPropertyId: reader.GetGuid(2),
            value: reader.GetStringOrDefault(3)
        );
    }
}
