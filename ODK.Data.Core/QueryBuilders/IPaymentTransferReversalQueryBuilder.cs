using ODK.Core.Payments;

namespace ODK.Data.Core.QueryBuilders;

public interface IPaymentTransferReversalQueryBuilder
    : IDatabaseEntityQueryBuilder<PaymentTransferReversal, IPaymentTransferReversalQueryBuilder>
{
    IPaymentTransferReversalQueryBuilder ForRefunds(IEnumerable<Guid> paymentRefundIds);
}
