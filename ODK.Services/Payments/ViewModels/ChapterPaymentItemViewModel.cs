using ODK.Core.Members;
using ODK.Core.Payments;

namespace ODK.Services.Payments.ViewModels;

/// <summary>
/// One row of a group's payments: what was taken, from whom, and what has since been given back.
/// </summary>
public class ChapterPaymentItemViewModel
{
    /// <summary>
    /// Whether a refund is already on the books - given back, or agreed to and not yet paid. Wider than
    /// <see cref="RefundedAmount"/>, so a refund in flight still counts as one the payment has.
    /// </summary>
    public required bool HasRefund { get; init; }

    public required Member Member { get; init; }

    public required Payment Payment { get; init; }

    /// <summary>
    /// What the provider has confirmed leaving us, in the payment's currency. Null where nothing has -
    /// which is not the same as a refund that has yet to be paid, and reads differently to the group.
    /// </summary>
    public required decimal? RefundedAmount { get; init; }
}
