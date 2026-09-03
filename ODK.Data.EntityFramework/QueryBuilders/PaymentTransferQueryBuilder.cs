using Microsoft.EntityFrameworkCore;
using ODK.Core.Payments;
using ODK.Data.Core.QueryBuilders;

namespace ODK.Data.EntityFramework.QueryBuilders;

public class PaymentTransferQueryBuilder
    : DatabaseEntityQueryBuilder<PaymentTransfer, IPaymentTransferQueryBuilder>, IPaymentTransferQueryBuilder
{
    public PaymentTransferQueryBuilder(DbContext context)
        : base(context)
    {
    }

    protected override IPaymentTransferQueryBuilder Builder => this;

    public IPaymentTransferQueryBuilder ForPayment(Guid paymentId)
    {
        Query = Query.Where(x => x.PaymentId == paymentId);
        return this;
    }

    public IPaymentTransferQueryBuilder ForPayments(IEnumerable<Guid> paymentIds)
    {
        Query = Query.Where(x => paymentIds.Contains(x.PaymentId));
        return this;
    }
}
