using ODK.Core.Features;

namespace ODK.Services.Subscriptions.ViewModels;

public class SiteSubscriptionSiteAdminListItemViewModel
{
    public required int ActiveCount { get; init; }

    public required bool Default { get; init; }

    public required bool Enabled { get; init; }

    public required IReadOnlyCollection<SiteFeatureType> Features { get; init; }

    public required int? GroupLimit { get; init; }

    public required Guid Id { get; init; }

    public required int? MemberLimit { get; init; }

    public required string Name { get; init; }

    public required string PaymentSettingsName { get; init; }

    public required IReadOnlyCollection<SiteSubscriptionSiteAdminListItemPriceViewModel> Prices { get; init; }

    public bool HasFeature(SiteFeatureType feature) => Features.Contains(feature);
}