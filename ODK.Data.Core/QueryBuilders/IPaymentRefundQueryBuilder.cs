using ODK.Core.Payments;

namespace ODK.Data.Core.QueryBuilders;

public interface IPaymentRefundQueryBuilder : IDatabaseEntityQueryBuilder<PaymentRefund, IPaymentRefundQueryBuilder>
{
    IPaymentRefundQueryBuilder ForPayment(Guid paymentId);

    IPaymentRefundQueryBuilder ForPayments(IEnumerable<Guid> paymentIds);

    /// <summary>
    /// Refunds that have reduced what the member paid, or are expected to: everything but the ones that
    /// were cancelled or that the provider failed. What a payment's remaining refundable amount is measured
    /// against.
    /// </summary>
    IPaymentRefundQueryBuilder Live();

    /// <summary>
    /// Refunds the provider has been given but has not confirmed the outcome of.
    /// </summary>
    IPaymentRefundQueryBuilder Unconfirmed();
}
