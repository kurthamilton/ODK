namespace ODK.Services.Integrations.Payments.Stripe;

public class StripePaymentProviderSettings
{
    public required string ConnectedAccountBaseUrl { get; init; }

    public required decimal ConnectedAccountCommissionPercentage { get; init; }

    /// <summary>
    /// The Business Profile industry code
    /// </summary>
    public required string ConnectedAccountMcc { get; init; }

    public required string ConnectedAccountProductDescription { get; init; }
}
