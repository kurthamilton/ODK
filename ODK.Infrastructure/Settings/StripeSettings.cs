using ODK.Core.Platforms;

namespace ODK.Infrastructure.Settings;

public class StripeSettings
{
    public required string ConnectedAccountBusinessName { get; init; }

    public required decimal ConnectedAccountCommissionPercentage { get; init; }

    public required string ConnectedAccountMcc { get; init; }

    public required string ConnectedAccountProductDescription { get; init; }

    public required Dictionary<PlatformType, StripePlatformSettings> Platforms { get; init; }

    public required int SettlementReadDelaySeconds { get; init; }

    public required StripeWebhooksSettings Webhooks { get; init; }
}
