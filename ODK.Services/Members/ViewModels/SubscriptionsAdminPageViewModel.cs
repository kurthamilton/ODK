using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Core.Platforms;
using ODK.Data.Core.Chapters;
using ODK.Services.Payments;

namespace ODK.Services.Members.ViewModels;

public class SubscriptionsAdminPageViewModel
{
    public required Chapter Chapter { get; init; }

    public required IReadOnlyCollection<ChapterSubscriptionAdminDto> ChapterSubscriptions { get; init; }

    public required ExternalSubscription? ExternalSubscription { get; init; }

    public required ChapterMembershipSettings MembershipSettings { get; init; }

    public required MemberSubscription? MemberSubscription { get; init; }

    public required MemberSiteSubscription? OwnerSubscription { get; init; }

    public required PlatformType Platform { get; init; }
}