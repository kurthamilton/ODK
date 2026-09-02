using ODK.Core.Payments;

namespace ODK.Services.Payments.ViewModels;

/// <summary>
/// What to offer on the record-a-refund form. Every value is a starting point rather than a decision: the
/// site admin has already made the refund at the provider and knows what it actually did.
/// </summary>
public class PaymentRefundCreateViewModel
{
    /// <summary>
    /// The whole of what the payment took, where a payment is named. A refund is usually all of it.
    /// </summary>
    public required decimal? Amount { get; init; }

    public required string? ChapterName { get; init; }

    /// <summary>
    /// The payment the form was opened against, where one was named and found. Null for the blank form,
    /// and for a payment this platform does not hold.
    /// </summary>
    public required Payment? Payment { get; init; }

    public required string? PaymentReference { get; init; }

    /// <summary>
    /// The whole of the group's share, which is what a full refund would reverse.
    /// </summary>
    public required decimal? ReversedAmount { get; init; }

    /// <summary>The viewing site admin's zone: a site-wide page has no chapter to fall back to.</summary>
    public required TimeZoneInfo TimeZone { get; init; }
}
