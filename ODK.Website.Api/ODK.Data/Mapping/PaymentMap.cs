using System.Data;
using ODK.Core.Payments;
using ODK.Data.Sql.Mapping;

namespace ODK.Data.Mapping
{
    public class PaymentMap : SqlMap<Payment>
    {
        public PaymentMap()
            : base("Payments")
        {
            Property(x => x.Id).HasColumnName("PaymentId").IsIdentity();
            Property(x => x.MemberId);
            Property(x => x.PaidDate);
            Property(x => x.CurrencyCode);
            Property(x => x.Amount).HasColumnType(SqlDbType.Money);
            Property(x => x.Reference);
        }

        public override Payment Read(IDataReader reader)
        {
            return new Payment
            (
                id: reader.GetGuid(0),
                memberId: reader.GetGuid(1),
                paidDate: reader.GetDateTime(2),
                currencyCode: reader.GetString(3),
                amount: reader.GetDouble(4),
                reference: reader.GetString(5)
            );
        }
    }
}
