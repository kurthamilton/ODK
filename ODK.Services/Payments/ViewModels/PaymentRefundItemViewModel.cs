using ODK.Core.Payments;

namespace ODK.Services.Payments.ViewModels;

public class PaymentRefundItemViewModel
{
    /// <summary>
    /// The group the refunded payment was taken for. Null for a site payment.
    /// </summary>
    public required string? ChapterName { get; init; }

    /// <summary>
    /// What the group still owes of this refund - what a reversal could not take back. Null for a site
    /// payment, which has no group to recover from.
    /// </summary>
    public required decimal? OutstandingAmount { get; init; }

    public required Payment Payment { get; init; }

    public required PaymentRefund Refund { get; init; }
}
