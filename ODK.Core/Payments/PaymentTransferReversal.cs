namespace ODK.Core.Payments;

/// <summary>
/// What a <see cref="PaymentRefund"/> recovered from the group by reversing its
/// <see cref="PaymentTransfer"/>.
/// </summary>
/// <remarks>
/// A transfer can be reversed more than once, because a payment can be refunded more than once, and the
/// sum of a transfer's reversals cannot exceed <see cref="PaymentTransfer.Amount"/>. Where a reversal
/// cannot cover what the group owes for the refund, the remainder is a <c>ChapterPaymentAdjustment</c>
/// recovered from the group's later transfers.
/// </remarks>
public class PaymentTransferReversal : IDatabaseEntity
{
    /// <summary>
    /// What the provider says actually came back, in the transfer's currency. Null until the reversal has
    /// been read back from the provider.
    /// </summary>
    public decimal? ActualAmount { get; set; }

    /// <summary>
    /// What we asked to take back, in the transfer's currency. Compare <see cref="ActualAmount"/>, which
    /// is what the provider says moved.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// When the provider confirmed the money came back. Null while it has not: a reversal with an
    /// <see cref="ExternalId"/> and no date here is one whose outcome we have yet to read.
    /// </summary>
    public DateTime? CompletedUtc { get; set; }

    public DateTime CreatedUtc { get; set; }

    /// <summary>
    /// The provider's reversal. Set as soon as the provider accepts it, which is before it confirms what
    /// it did.
    /// </summary>
    public string? ExternalId { get; set; }

    public Guid Id { get; set; }

    /// <summary>
    /// The refund this was raised to cover. Ours rather than the provider's: the provider knows only that
    /// a transfer was reversed, not what it was reversed for.
    /// </summary>
    public Guid PaymentRefundId { get; set; }

    public Guid PaymentTransferId { get; set; }
}
