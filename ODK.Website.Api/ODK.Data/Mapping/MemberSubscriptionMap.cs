using System.Data;
using ODK.Core.Members;
using ODK.Data.Sql;
using ODK.Data.Sql.Mapping;

namespace ODK.Data.Mapping
{
    public class MemberSubscriptionMap : SqlMap<MemberSubscription>
    {
        public MemberSubscriptionMap()
            : base("Members")
        {
            Property(x => x.MemberId);
            Property(x => x.Type).HasColumnName("SubscriptionTypeId");
            Property(x => x.ExpiryDate).HasColumnName("SubscriptionExpiryDate");
        }

        public override MemberSubscription Read(IDataReader reader)
        {
            return new MemberSubscription
            (
                memberId: reader.GetGuid(0),
                type: (SubscriptionType)reader.GetInt32(1),
                expiryDate: reader.GetDateTimeOrDefault(2)
            );
        }
    }
}
