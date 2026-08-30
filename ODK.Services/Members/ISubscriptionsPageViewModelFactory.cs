using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Services.Members.ViewModels;

namespace ODK.Services.Members;

/// <summary>
/// Builds the subscription view model both platforms render from rows the caller has already loaded, so a
/// caller that needs other data alongside it can batch every query into a single round-trip.
/// </summary>
public interface ISubscriptionsPageViewModelFactory
{
    /// <param name="chapterSubscriptions">
    /// Every tier, disabled ones included. The visible-only filtering happens here, after the member's
    /// current tier has been resolved.
    /// </param>
    Task<SubscriptionsPageViewModel> Create(
        IMemberChapterServiceRequest request,
        MemberChapterSubscription? memberSubscription,
        IReadOnlyCollection<ChapterSubscription> chapterSubscriptions,
        MemberSubscriptionRecord? memberSubscriptionRecord,
        ChapterMembershipSettings? membershipSettings);
}
