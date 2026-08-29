using ODK.Core.Features;
using ODK.Core.Payments;
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

    /// <summary>
    /// The deployment the subscription was created under. A site admin sees only this deployment's
    /// subscriptions, so it is stated to make an unstamped legacy row visible rather than invisible.
    /// </summary>
    public required EnvironmentType? Environment { get; init; }

    public required IReadOnlyCollection<SiteFeatureType> Features { get; init; }

    public required bool Free { get; init; }

    public required int? GroupLimit { get; init; }

    public required Guid Id { get; init; }

    public required int? MemberLimit { get; init; }

    public required string Name { get; init; }

    public required PaymentProviderType? PaymentProvider { get; init; }

    public required IReadOnlyCollection<SiteSubscriptionSiteAdminListItemPriceViewModel> Prices { get; init; }

    public bool HasFeature(SiteFeatureType feature) => Features.Contains(feature);
}