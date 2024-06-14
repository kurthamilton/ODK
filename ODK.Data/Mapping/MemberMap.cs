using System.Data;
using ODK.Core.Members;
using ODK.Data.Sql.Mapping;

namespace ODK.Data.Mapping;

public class MemberMap : SqlMap<Member>
{
    public MemberMap()
        : base("Members")
    {
        Property(x => x.Id).HasColumnName("MemberId").IsIdentity();
        Property(x => x.ChapterId);
        Property(x => x.EmailAddress);
        Property(x => x.EmailOptIn);
        Property(x => x.FirstName);
        Property(x => x.LastName);
        Property(x => x.CreatedDate);
        Property(x => x.Activated);
        Property(x => x.Disabled);
        Property(x => x.Version).IsRowVersion();
    }

    public override Member Read(IDataReader reader)
    {
        return new Member
        (
            id: reader.GetGuid(0),
            chapterId: reader.GetGuid(1),
            emailAddress: reader.GetString(2),
            emailOptIn: reader.GetBoolean(3),
            firstName: reader.GetString(4),
            lastName: reader.GetString(5),
            createdDate: reader.GetDateTime(6),
            activated: reader.GetBoolean(7),
            disabled: reader.GetBoolean(8),
            version: reader.GetInt64(9)
        );
    }
}
