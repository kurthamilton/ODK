using ODK.Core.Payments;

namespace ODK.Services.Payments.Models;

/// <summary>
/// A refund to make through the payment provider.
/// </summary>
public class RefundPaymentModel
{
    /// <summary>
    /// What to give the member back, in the payment's currency. Cannot exceed what the payment took, less
    /// whatever it has already given back. Null refunds the whole of it.
    /// </summary>
    public required decimal? Amount { get; init; }

    /// <inheritdoc cref="PaymentRefund.Reason"/>
    public required string Reason { get; init; }
}
