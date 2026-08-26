using ODK.Core.Features;
using ODK.Core.Platforms;

namespace ODK.Services.Subscriptions.ViewModels;

public class SiteSubscriptionSiteAdminListItemViewModel
{
    public required int ActiveCount { get; init; }

    /// <summary>
    /// Whether nothing stands in the way of deleting the subscription, so the list can offer it. The rule
    /// is enforced by <c>ISiteSubscriptionAdminService.DeleteSiteSubscription</c>, which names what is
    /// blocking instead - a change to one is a change to the other.
    /// </summary>
    public required bool CanDelete { get; init; }

    public required bool Default { get; init; }

    public required bool Enabled { get; init; }

    public required IReadOnlyCollection<SiteFeatureType> Features { get; init; }

    public required bool Free { get; init; }

    public required int? GroupLimit { get; init; }

    public required Guid Id { get; init; }

    public required int? MemberLimit { get; init; }

    public required string Name { get; init; }

    public required Guid PaymentSettingsId { get; init; }

    public required string PaymentSettingsName { get; init; }

    /// <summary>
    /// Whether the subscription's payment settings belong to a platform other than the one being
    /// administered. A subscription transacts through its payment settings account, so a mismatch means it
    /// takes money under another platform's keys.
    /// </summary>
    public required bool PaymentSettingsOnAnotherPlatform { get; init; }

    public required PlatformType PaymentSettingsPlatform { get; init; }

    public required IReadOnlyCollection<SiteSubscriptionSiteAdminListItemPriceViewModel> Prices { get; init; }

    public bool HasFeature(SiteFeatureType feature) => Features.Contains(feature);
}