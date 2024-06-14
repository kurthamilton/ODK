using System.Data;
using ODK.Core.Members;
using ODK.Data.Sql.Mapping;

namespace ODK.Data.Mapping;

public class MemberPasswordMap : SqlMap<MemberPassword>
{
    public MemberPasswordMap()
        : base("Members")
    {
        Property(x => x.MemberId).IsIdentity();
        Property(x => x.Password);
        Property(x => x.Salt).HasColumnName("PasswordSalt");
    }

    public override MemberPassword Read(IDataReader reader)
    {
        return new MemberPassword
        (
            memberId: reader.GetGuid(0),
            password: reader.GetString(1),
            salt: reader.GetString(2)
        );
    }
}
