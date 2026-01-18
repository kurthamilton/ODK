using ODK.Core.Features;

namespace ODK.Services.Subscriptions;

public class SiteSubscriptionCreateModel
{
    public required string Description { get; init; } = string.Empty;

    public required bool Enabled { get; init; }

    public required Guid? FallbackSiteSubscriptionId { get; init; }

    public required IReadOnlyCollection<SiteFeatureType> Features { get; init; }

    public required int? GroupLimit { get; init; }

    public required int? MemberLimit { get; init; }

    public required string Name { get; init; } = string.Empty;

    public required Guid SitePaymentSettingId { get; init; }
}