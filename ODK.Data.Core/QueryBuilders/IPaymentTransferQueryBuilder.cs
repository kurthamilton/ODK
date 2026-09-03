using ODK.Core.Payments;

namespace ODK.Data.Core.QueryBuilders;

public interface IPaymentTransferQueryBuilder
    : IDatabaseEntityQueryBuilder<PaymentTransfer, IPaymentTransferQueryBuilder>
{
    IPaymentTransferQueryBuilder ForPayment(Guid paymentId);

    IPaymentTransferQueryBuilder ForPayments(IEnumerable<Guid> paymentIds);
}
