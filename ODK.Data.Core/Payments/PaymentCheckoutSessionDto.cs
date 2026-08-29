using ODK.Core.Payments;

namespace ODK.Data.Core.Payments;

public class PaymentCheckoutSessionDto
{
    public required Payment Payment { get; init; }

    public required PaymentCheckoutSession Session { get; init; }
}
