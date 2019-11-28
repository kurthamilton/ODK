using System.Data;
using ODK.Core.Payments;
using ODK.Data.Sql.Mapping;

namespace ODK.Data.Mapping
{
    public class SubscriptionTypeMap : SqlMap<SubscriptionType>
    {
        public SubscriptionTypeMap()
            : base("SubscriptionTypes")
        {
            Property(x => x.Id).HasColumnName("SubscriptionTypeId").IsIdentity();
            Property(x => x.Name);
        }

        public override SubscriptionType Read(IDataReader reader)
        {
            return new SubscriptionType
            (
                id: reader.GetGuid(0),
                name: reader.GetString(1)
            );
        }
    }
}
