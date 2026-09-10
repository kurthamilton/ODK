using ODK.Core.Chapters;
using ODK.Core.Countries;
using ODK.Core.Members;
using ODK.Data.Core.Members;
using ODK.Services.Payments;

namespace ODK.Services.Subscriptions.ViewModels;

public class SiteSubscriptionsViewModel
{
    public required Chapter? Chapter { get; init; }

    public required IReadOnlyCollection<Currency> Currencies { get; init; }

    public required Currency? Currency { get; init; }

    public required Member? CurrentMember { get; init; }

    public required ExternalSubscription? CurrentMemberExternalSubscription { get; init; }

    public required MemberSiteSubscriptionDto? CurrentMemberSubscription { get; init; }

    public required IReadOnlyCollection<SiteSubscriptionListItemViewModel> Subscriptions { get; init; }
}
