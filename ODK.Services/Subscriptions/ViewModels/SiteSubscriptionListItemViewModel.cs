using ODK.Core.Chapters;
using ODK.Core.Platforms;
using ODK.Core.Subscriptions;

namespace ODK.Services.Subscriptions.ViewModels;

public class SiteSubscriptionListItemViewModel
{
    public required Chapter? Chapter { get; init; }

    public required bool IsCurrentMemberActiveSubscription { get; init; }

    public PlatformType Platform { get; init; }

    public required IReadOnlyCollection<SiteSubscriptionPrice> Prices { get; init; }

    public required SiteSubscription Subscription { get; init; }
}
