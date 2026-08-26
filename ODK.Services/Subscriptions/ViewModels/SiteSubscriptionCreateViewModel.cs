using ODK.Core.Payments;
using ODK.Core.Subscriptions;

namespace ODK.Services.Subscriptions.ViewModels;

public class SiteSubscriptionCreateViewModel
{
    public required IReadOnlyCollection<SitePaymentSettings> SitePaymentSettings { get; init; }

    public required IReadOnlyCollection<SiteSubscription> Subscriptions { get; init; }
}
