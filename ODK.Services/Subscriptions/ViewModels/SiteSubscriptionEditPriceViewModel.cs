using ODK.Core.Subscriptions;

namespace ODK.Services.Subscriptions.ViewModels;

public class SiteSubscriptionEditPriceViewModel
{
    public required decimal Amount { get; init; }

    /// <summary>
    /// Whether nothing stands in the way of deleting the price, so the list can offer it. The rule is
    /// enforced by <c>ISiteSubscriptionAdminService.DeleteSiteSubscriptionPrice</c>, which names what is
    /// blocking instead - a change to one is a change to the other.
    /// </summary>
    public required bool CanDelete { get; init; }

    public required Guid CurrencyId { get; init; }

    public required string? ExternalId { get; init; }

    public required SiteSubscriptionFrequency Frequency { get; init; }

    public required Guid Id { get; init; }
}
