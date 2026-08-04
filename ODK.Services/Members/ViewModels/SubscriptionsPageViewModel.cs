using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Services.Payments;

namespace ODK.Services.Members.ViewModels;

public class SubscriptionsPageViewModel
{
    public required Chapter Chapter { get; init; }

    public required IReadOnlyCollection<ChapterSubscription> ChapterSubscriptions { get; init; }

    public required Member CurrentMember { get; init; }

    public required ChapterSubscription? CurrentSubscription { get; init; }

    public required ExternalSubscription? ExternalSubscription { get; init; }

    public required ChapterMembershipSettings? MembershipSettings { get; init; }

    public required MemberChapterSubscription? MemberSubscription { get; init; }
}