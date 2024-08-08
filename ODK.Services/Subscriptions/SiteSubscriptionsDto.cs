using ODK.Core.Countries;

namespace ODK.Services.Subscriptions;

public class SiteSubscriptionsDto
{
    public required IReadOnlyCollection<Currency> Currencies { get; init; }

    public required IReadOnlyCollection<SiteSubscriptionDto> Subscriptions { get; init; }
}
