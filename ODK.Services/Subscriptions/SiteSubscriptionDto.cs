using ODK.Core.Countries;
using ODK.Core.Subscriptions;

namespace ODK.Services.Subscriptions;

public class SiteSubscriptionDto
{
    public required IReadOnlyCollection<Currency> Currencies { get; init; }

    public required IReadOnlyCollection<SiteSubscriptionPrice> Prices { get; init; }

    public required SiteSubscription Subscription { get; init; }
}
