using ODK.Core.Payments;

namespace ODK.Data.Core.Payments;

public class PaymentReconciliationDto
{
    public required Payment Payment { get; init; }

    /// <summary>
    /// Null where no reconcile has had anything to say about the payment, which is the ordinary case.
    /// </summary>
    public required PaymentReconciliation? Reconciliation { get; init; }
}
