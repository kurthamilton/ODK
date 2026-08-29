using ODK.Core.Payments;

namespace ODK.Infrastructure.Settings;

public class PaymentsSettings
{
    public required PaymentProviderType Active { get; init; }

    public required PaymentsPayPalSettings PayPal { get; init; }

    public required PaymentsStripeSettings Stripe { get; init; }
}
