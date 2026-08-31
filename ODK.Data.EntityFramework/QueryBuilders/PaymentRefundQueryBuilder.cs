using Microsoft.EntityFrameworkCore;
using ODK.Core.Payments;
using ODK.Data.Core.QueryBuilders;

namespace ODK.Data.EntityFramework.QueryBuilders;

public class PaymentRefundQueryBuilder
    : DatabaseEntityQueryBuilder<PaymentRefund, IPaymentRefundQueryBuilder>, IPaymentRefundQueryBuilder
{
    public PaymentRefundQueryBuilder(DbContext context)
        : base(context)
    {
    }

    protected override IPaymentRefundQueryBuilder Builder => this;

    public IPaymentRefundQueryBuilder ForPayment(Guid paymentId)
    {
        Query = Query.Where(x => x.PaymentId == paymentId);
        return this;
    }

    public IPaymentRefundQueryBuilder ForPayments(IEnumerable<Guid> paymentIds)
    {
        Query = Query.Where(x => paymentIds.Contains(x.PaymentId));
        return this;
    }

    public IPaymentRefundQueryBuilder Live()
    {
        Query = Query.Where(x =>
            x.Status != PaymentRefundStatusType.Declined &&
            x.Status != PaymentRefundStatusType.Failed);
        return this;
    }

    public IPaymentRefundQueryBuilder Unconfirmed()
    {
        Query = Query.Where(x => x.Status == PaymentRefundStatusType.Refunding);
        return this;
    }
}
