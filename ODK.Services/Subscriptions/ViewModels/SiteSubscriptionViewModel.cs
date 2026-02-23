using ODK.Core.Countries;
using ODK.Core.Members;
using ODK.Core.Payments;
using ODK.Core.Subscriptions;
using ODK.Services.Payments;

namespace ODK.Services.Subscriptions.ViewModels;

public class SiteSubscriptionViewModel
{
    public required IReadOnlyCollection<Currency> Currencies { get; init; }

    public required ExternalSubscription? CurrentMemberExternalSubscription { get; init; }

    public required MemberSiteSubscription? CurrentMemberSiteSubscription { get; init; }

    public required IReadOnlyCollection<SiteSubscriptionPrice> Prices { get; init; }

    public required IReadOnlyCollection<SitePaymentSettings> SitePaymentSettings { get; init; }

    public required SiteSubscription Subscription { get; init; }
}
