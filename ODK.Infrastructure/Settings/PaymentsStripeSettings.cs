using ODK.Core.Platforms;

namespace ODK.Infrastructure.Settings;

public class PaymentsStripeSettings
{
    public required string ConnectedAccountBusinessName { get; init; }

    public required decimal ConnectedAccountCommissionPercentage { get; init; }

    public required string ConnectedAccountMcc { get; init; }

    public required string ConnectedAccountProductDescription { get; init; }

    public required Dictionary<PlatformType, PaymentsStripePlatformSettings> Platforms { get; init; }
}
