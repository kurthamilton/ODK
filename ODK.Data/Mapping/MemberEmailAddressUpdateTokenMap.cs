using System.Data;
using ODK.Core.Members;
using ODK.Data.Sql.Mapping;

namespace ODK.Data.Mapping
{
    public class MemberEmailAddressUpdateTokenMap : SqlMap<MemberEmailAddressUpdateToken>
    {
        public MemberEmailAddressUpdateTokenMap()
            : base("MemberEmailAddressUpdateTokens")
        {
            Property(x => x.MemberId);
            Property(x => x.NewEmailAddress);
            Property(x => x.ConfirmationToken);
        }

        public override MemberEmailAddressUpdateToken Read(IDataReader reader)
        {
            return new MemberEmailAddressUpdateToken
            (
                memberId: reader.GetGuid(0),
                newEmailAddress: reader.GetString(1),
                confirmationToken: reader.GetString(2)
            );
        }
    }
}
