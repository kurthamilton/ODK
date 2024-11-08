using ODK.Core.Countries;
using ODK.Core.Payments;
using ODK.Core.Subscriptions;

namespace ODK.Services.Subscriptions.ViewModels;

public class SiteSubscriptionViewModel
{
    public required IReadOnlyCollection<Currency> Currencies { get; init; }

    public required IReadOnlyCollection<SiteSubscriptionPrice> Prices { get; init; }

    public required IReadOnlyCollection<SitePaymentSettings> SitePaymentSettings { get; init; }

    public required SiteSubscription Subscription { get; init; }
}
