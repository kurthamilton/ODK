namespace ODK.Infrastructure.Settings;

public class StripeSettings
{
    public required string ConnectedAccountBusinessName { get; init; }

    public required decimal ConnectedAccountCommissionPercentage { get; init; }

    public required string ConnectedAccountMcc { get; init; }

    public required string ConnectedAccountProductDescription { get; init; }

    public required StripeDashboardSettings Dashboard { get; init; }

    public required Dictionary<PlatformKey, StripePlatformSettings> Platforms { get; init; }

    public required int SettlementReadDelaySeconds { get; init; }

    public required StripeWebhooksSettings Webhooks { get; init; }
}
