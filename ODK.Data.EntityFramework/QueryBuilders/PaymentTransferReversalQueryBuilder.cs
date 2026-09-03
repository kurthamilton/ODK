using Microsoft.EntityFrameworkCore;
using ODK.Core.Payments;
using ODK.Data.Core.QueryBuilders;

namespace ODK.Data.EntityFramework.QueryBuilders;

public class PaymentTransferReversalQueryBuilder
    : DatabaseEntityQueryBuilder<PaymentTransferReversal, IPaymentTransferReversalQueryBuilder>,
    IPaymentTransferReversalQueryBuilder
{
    public PaymentTransferReversalQueryBuilder(DbContext context)
        : base(context)
    {
    }

    protected override IPaymentTransferReversalQueryBuilder Builder => this;

    public IPaymentTransferReversalQueryBuilder ForRefunds(IEnumerable<Guid> paymentRefundIds)
    {
        Query = Query.Where(x => paymentRefundIds.Contains(x.PaymentRefundId));
        return this;
    }
}
