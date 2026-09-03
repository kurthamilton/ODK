using ODK.Core.Members;
using ODK.Core.Payments;

namespace ODK.Services.Payments.ViewModels;

/// <summary>
/// One row of a payments table: what was taken, from whom, and what has since been given back. Shared by
/// the group's own payments page and the site admin's, which differ in the columns around the row rather
/// than in the row itself.
/// </summary>
public class PaymentItemViewModel
{
    /// <summary>
    /// The group's share of the payment, in the payment's currency. Null until the settlement has been
    /// read, and for a payment taken by the site.
    /// </summary>
    public required decimal? ChapterAmount { get; init; }

    /// <summary>
    /// The group the payment was taken for. Null for a payment taken by the site, and on a page already
    /// scoped to one group, which has no column for it.
    /// </summary>
    public required string? ChapterName { get; init; }

    /// <summary>
    /// Whether a refund is already on the books - given back, or agreed to and not yet paid. Wider than
    /// <see cref="RefundedAmount"/>, so a refund in flight still counts as one the payment has.
    /// </summary>
    public required bool HasRefund { get; init; }

    public required Member Member { get; init; }

    public required Payment Payment { get; init; }

    /// <summary>
    /// What can still be given back through the payment provider, in the payment's currency: what the
    /// payment took, less what its live refunds have already claimed. Null where a refund cannot be made
    /// at all - the payment has never been settled, or names no charge to refund - and zero where the
    /// whole of it has been given back.
    /// </summary>
    public required decimal? RefundableAmount { get; init; }

    /// <summary>
    /// What the provider has confirmed leaving us, in the payment's currency. Null where nothing has -
    /// which is not the same as a refund that has yet to be paid, and reads differently to the group.
    /// </summary>
    public required decimal? RefundedAmount { get; init; }
}
