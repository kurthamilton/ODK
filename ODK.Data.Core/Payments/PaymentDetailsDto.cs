using ODK.Core.Payments;

namespace ODK.Data.Core.Payments;

/// <summary>
/// One payment and everything recorded against it: the group's share, what has been given back, what came
/// back off the group to cover it, and what the reconciliation job has to say.
/// </summary>
/// <remarks>
/// The concerns are separate entities so a payment row stays narrow, and this composes them back for a
/// caller that needs the whole picture rather than making it fetch each one. Where a caller needs only
/// part of it - a page listing what is left to reconcile, say - <c>WithReconciliation</c> stays the
/// thinner read.
/// </remarks>
public class PaymentDetailsDto
{
    public required Payment Payment { get; init; }

    /// <summary>
    /// Null where no reconcile has ever had anything to say about the payment, which is the ordinary case.
    /// </summary>
    public required PaymentReconciliation? Reconciliation { get; init; }

    /// <summary>
    /// Every refund of the payment, including the ones that were cancelled or that the provider failed.
    /// Which of them count towards what is left is <see cref="Payment.RefundableAmount"/>'s to decide.
    /// </summary>
    public required IReadOnlyCollection<PaymentRefund> Refunds { get; init; }

    /// <summary>
    /// Everything already taken back off <see cref="Transfer"/>. Empty where there was no transfer.
    /// </summary>
    public required IReadOnlyCollection<PaymentTransferReversal> Reversals { get; init; }

    /// <summary>
    /// The group's share. Null for a payment the site took for itself, and for one whose settlement has
    /// never been read - the share is not worked out until it has.
    /// </summary>
    public required PaymentTransfer? Transfer { get; init; }

    /// <inheritdoc cref="Payment.RefundableAmount"/>
    public decimal? RefundableAmount => Payment.RefundableAmount(Refunds);

    /// <inheritdoc cref="PaymentTransfer.ReversibleAmount"/>
    public decimal ReversibleAmount => Transfer?.ReversibleAmount(Reversals) ?? 0;
}
