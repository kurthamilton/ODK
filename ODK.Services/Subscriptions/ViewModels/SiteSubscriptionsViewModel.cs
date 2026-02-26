using ODK.Core.Countries;
using ODK.Core.Members;
using ODK.Core.Payments;
using ODK.Core.Subscriptions;
using ODK.Data.Core.Members;
using ODK.Services.Payments;

namespace ODK.Services.Subscriptions.ViewModels;

public class SiteSubscriptionsViewModel
{
    public required IReadOnlyCollection<Currency> Currencies { get; init; }

    public required Currency? Currency { get; init; }

    public required Member CurrentMember { get; init; }

    public required ExternalSubscription? CurrentMemberExternalSubscription { get; init; }

    public required MemberSiteSubscriptionDto? CurrentMemberSubscription { get; init; }

    public required IReadOnlyCollection<SitePaymentSettings> SitePaymentSettings { get; init; }

    public required IReadOnlyCollection<SiteSubscriptionViewModel> Subscriptions { get; init; }
}
