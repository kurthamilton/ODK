using ODK.Core.Chapters;
using ODK.Core.Members;

namespace ODK.Services.Members.ViewModels;

public class MemberSubscriptionAlertViewModel
{
    public required ChapterMembershipSettings? ChapterMembershipSettings { get; init; }

    public required MemberSubscription? MemberSubscription { get; init; }
}