namespace ODK.Web.Common.Config.Settings;

public class PaymentsSettings
{
    public required PaymentsPayPalSettings PayPal { get; set; }

    public required PaymentsStripeSettings Stripe { get; set; }
}
