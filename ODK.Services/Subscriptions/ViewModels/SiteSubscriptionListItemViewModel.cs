using ODK.Core.Subscriptions;

namespace ODK.Services.Subscriptions.ViewModels;

public class SiteSubscriptionListItemViewModel
{
    public required bool IsCurrentMemberActiveSubscription { get; init; }

    public required IReadOnlyCollection<SiteSubscriptionPrice> Prices { get; init; }

    public required SiteSubscription Subscription { get; init; }
}
