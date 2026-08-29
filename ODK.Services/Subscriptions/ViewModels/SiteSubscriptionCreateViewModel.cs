using ODK.Core.Subscriptions;

namespace ODK.Services.Subscriptions.ViewModels;

public class SiteSubscriptionCreateViewModel
{
    public required IReadOnlyCollection<SiteSubscription> Subscriptions { get; init; }
}
