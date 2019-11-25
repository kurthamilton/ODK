using System.Data;
using ODK.Core.Members;
using ODK.Data.Sql.Mapping;

namespace ODK.Data.Mapping
{
    public class MemberRefreshTokenMap : SqlMap<MemberRefreshToken>
    {
        public MemberRefreshTokenMap()
            : base("MemberRefreshTokens")
        {
            Property(x => x.Id).HasColumnName("MemberRefreshTokenId").IsIdentity();
            Property(x => x.MemberId);
            Property(x => x.Expires);
            Property(x => x.RefreshToken);
        }

        public override MemberRefreshToken Read(IDataReader reader)
        {
            return new MemberRefreshToken
            (
                id: reader.GetGuid(0),
                memberId: reader.GetGuid(1),
                expires: reader.GetDateTime(2),
                refreshToken: reader.GetString(3)
            );
        }
    }
}
