using ODK.Core.Chapters;
using ODK.Core.Members;

namespace ODK.Services.Members.ViewModels;

public class MemberSubscriptionAlertViewModel
{
    public required ChapterMembershipSettings? ChapterMembershipSettings { get; init; }

    /// <summary>
    /// The member has a recurring subscription that hasn't been cancelled, so it will auto-renew - no
    /// expiry warning should be shown.
    /// </summary>
    public required bool HasActiveRecurringSubscription { get; init; }

    public required MemberSubscription? MemberSubscription { get; init; }
}