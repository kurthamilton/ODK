using ODK.Core.Payments;

namespace ODK.Services.Payments.ViewModels;

public class PaymentRefundItemViewModel
{
    /// <summary>
    /// The group the refunded payment was taken for. Null for a site payment.
    /// </summary>
    public required string? ChapterName { get; init; }

    /// <summary>
    /// What the group still owes of this refund, read from the adjustment the refund raised rather than
    /// computed from the refund. A later transfer pays that adjustment down, so anything worked out from
    /// the refund alone would go on claiming a debt that has since been collected.
    /// </summary>
    /// <remarks>
    /// Null where the refund raised no adjustment: a site payment, or one whose reversal took back the
    /// whole of the group's share. Zero where the debt has since been recovered in full.
    /// </remarks>
    public required decimal? OutstandingAmount { get; init; }

    public required Payment Payment { get; init; }

    public required PaymentRefund Refund { get; init; }

    /// <summary>
    /// What was taken back off the group by reversing the payment's transfer. Null where nothing was: a
    /// site payment, one whose share never moved, or a reversal the provider refused.
    /// </summary>
    public required decimal? ReversedAmount { get; init; }
}
