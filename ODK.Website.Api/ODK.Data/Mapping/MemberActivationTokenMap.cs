using System.Data;
using ODK.Core.Members;
using ODK.Data.Sql.Mapping;

namespace ODK.Data.Mapping
{
    public class MemberActivationTokenMap : SqlMap<MemberActivationToken>
    {
        public MemberActivationTokenMap()
            : base("MemberActivationTokens")
        {
            Property(x => x.MemberId);
            Property(x => x.ActivationToken);
        }

        public override MemberActivationToken Read(IDataReader reader)
        {
            return new MemberActivationToken
            (
                memberId: reader.GetGuid(0),
                activationToken: reader.GetString(1)
            );
        }
    }
}
