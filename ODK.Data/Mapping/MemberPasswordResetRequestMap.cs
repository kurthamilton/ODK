using System.Data;
using ODK.Core.Members;
using ODK.Data.Sql.Mapping;

namespace ODK.Data.Mapping;

public class MemberPasswordResetRequestMap : SqlMap<MemberPasswordResetRequest>
{
    public MemberPasswordResetRequestMap()
        : base("MemberPasswordResetRequests")
    {
        Property(x => x.Id).HasColumnName("MemberPasswordResetRequestId").IsIdentity();
        Property(x => x.MemberId);
        Property(x => x.Created);
        Property(x => x.Expires);
        Property(x => x.Token);
    }

    public override MemberPasswordResetRequest Read(IDataReader reader)
    {
        return new MemberPasswordResetRequest
        (
            id: reader.GetGuid(0),
            memberId: reader.GetGuid(1),
            created: reader.GetDateTime(2),
            expires: reader.GetDateTime(3),
            token: reader.GetString(4)
        );
    }
}
