using ODK.Core.Chapters;
using ODK.Core.Platforms;
using ODK.Core.Subscriptions;
using ODK.Data.Core.Chapters;

namespace ODK.Services.Members.ViewModels;

public class SubscriptionsAdminPageViewModel
{
    public required Chapter Chapter { get; init; }

    public required IReadOnlyCollection<ChapterSubscriptionAdminDto> ChapterSubscriptions { get; init; }

    public required ChapterMembershipSettings MembershipSettings { get; init; }

    public required SiteSubscription? OwnerSubscription { get; init; }

    public required PlatformType Platform { get; init; }
}