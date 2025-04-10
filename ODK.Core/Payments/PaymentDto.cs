using ODK.Core.Countries;

namespace ODK.Core.Payments;

public class PaymentDto
{
    public required Currency Currency { get; init; }

    public required Payment Payment { get; init; }

    public required PaymentReconciliation? PaymentReconciliation { get; init; }
}
