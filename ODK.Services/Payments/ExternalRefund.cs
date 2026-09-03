using ODK.Core.Payments;

namespace ODK.Services.Payments;

/// <summary>
/// The provider's refund of a charge: money given back to whoever paid it.
/// </summary>
public class ExternalRefund
{
    public required decimal Amount { get; init; }

    public required DateTime CreatedUtc { get; init; }

    public required string CurrencyCode { get; init; }

    public required string ExternalId { get; init; }

    /// <summary>
    /// The provider's own view of where the refund has got to, mapped onto ours. A refund the provider has
    /// accepted but not yet confirmed is <see cref="PaymentRefundStatusType.Pending"/>.
    /// </summary>
    public required PaymentRefundStatusType Status { get; init; }
}
