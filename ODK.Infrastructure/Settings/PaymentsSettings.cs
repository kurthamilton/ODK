namespace ODK.Infrastructure.Settings;

public class PaymentsSettings
{
    public required PaymentsPayPalSettings PayPal { get; init; }

    public required PaymentsStripeSettings Stripe { get; init; }
}
