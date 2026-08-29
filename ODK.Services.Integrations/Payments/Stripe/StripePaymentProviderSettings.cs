using ODK.Core.Platforms;
using ODK.Services.Payments;

namespace ODK.Services.Integrations.Payments.Stripe;

public class StripePaymentProviderSettings
{
    public required string ConnectedAccountBusinessName { get; init; }

    public required decimal ConnectedAccountCommissionPercentage { get; init; }

    /// <summary>
    /// The Business Profile industry code
    /// </summary>
    public required string ConnectedAccountMcc { get; init; }

    public required string ConnectedAccountProductDescription { get; init; }

    public required IReadOnlyDictionary<PlatformType, StripePaymentProviderPlatformSettings> Platforms { get; init; }

    /// <inheritdoc cref="IPaymentProvider.SettlementReadDelay"/>
    public required TimeSpan SettlementReadDelay { get; init; }
}
