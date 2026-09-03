using ODK.Core.Payments;

namespace ODK.Data.Core.QueryBuilders;

public interface IPaymentReconciliationQueryBuilder
    : IDatabaseEntityQueryBuilder<PaymentReconciliation, IPaymentReconciliationQueryBuilder>
{
    IPaymentReconciliationQueryBuilder ForPayment(Guid paymentId);

    IPaymentReconciliationQueryBuilder ForPayments(IEnumerable<Guid> paymentIds);
}
