using System.Data;
using ODK.Core.Members;
using ODK.Data.Sql.Mapping;

namespace ODK.Data.Mapping
{
    public class MemberSubscriptionRecordMap : SqlMap<MemberSubscriptionRecord>
    {
        public MemberSubscriptionRecordMap()
            : base("MemberSubscriptionLog")
        {
            Property(x => x.MemberId);
            Property(x => x.Type);
            Property(x => x.PurchaseDate);
        }

        public override MemberSubscriptionRecord Read(IDataReader reader)
        {
            return new MemberSubscriptionRecord
            (
                memberId: reader.GetGuid(0),
                type: (SubscriptionType)reader.GetInt32(1),
                purchaseDate: reader.GetDateTime(2)
            );
        }
    }
}
