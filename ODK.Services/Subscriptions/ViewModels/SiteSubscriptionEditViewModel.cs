using ODK.Core.Payments;
using ODK.Core.Subscriptions;
using ODK.Data.Core.Countries;

namespace ODK.Services.Subscriptions.ViewModels;

public class SiteSubscriptionEditViewModel
{
    public required IReadOnlyCollection<CurrencyDto> Currencies { get; init; }

    public required IReadOnlyCollection<SiteSubscriptionFeature> Features { get; init; }

    public required IReadOnlyCollection<SiteSubscriptionEditPriceViewModel> Prices { get; init; }

    public required SiteSubscription Subscription { get; init; }

    public required IReadOnlyCollection<SiteSubscription> Subscriptions { get; init; }
}
