using ODK.Core.Payments;

namespace ODK.Services.Payments.ViewModels;

/// <summary>
/// One payment waiting to be reconciled, and what it is waiting for.
/// </summary>
public class PaymentReconciliationItemViewModel
{
    /// <summary>
    /// The group the payment was taken for. Null for a site payment, and for one naming a group that no
    /// longer exists.
    /// </summary>
    public required string? ChapterName { get; init; }

    public required Payment Payment { get; init; }

    public required PaymentReconciliationType Pending { get; init; }
}
