using Microsoft.EntityFrameworkCore;
using ODK.Core.Payments;
using ODK.Data.Core.QueryBuilders;

namespace ODK.Data.EntityFramework.QueryBuilders;

public class PaymentReconciliationQueryBuilder
    : DatabaseEntityQueryBuilder<PaymentReconciliation, IPaymentReconciliationQueryBuilder>,
    IPaymentReconciliationQueryBuilder
{
    public PaymentReconciliationQueryBuilder(DbContext context)
        : base(context)
    {
    }

    protected override IPaymentReconciliationQueryBuilder Builder => this;

    public IPaymentReconciliationQueryBuilder ForPayment(Guid paymentId)
    {
        Query = Query.Where(x => x.PaymentId == paymentId);
        return this;
    }

    public IPaymentReconciliationQueryBuilder ForPayments(IEnumerable<Guid> paymentIds)
    {
        Query = Query.Where(x => paymentIds.Contains(x.PaymentId));
        return this;
    }
}
