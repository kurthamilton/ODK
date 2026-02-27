using ODK.Core.Chapters;
using ODK.Core.Events;
using ODK.Core.Features;
using ODK.Core.Members;
using ODK.Core.Subscriptions;
using ODK.Core.Venues;

namespace ODK.Services.Authorization;

public interface IAuthorizationService
{
    bool CanRespondToEvent(
        Event @event,
        Member? member,
        MemberSubscription? subscription,
        ChapterMembershipSettings? membershipSettings,
        ChapterPrivacySettings? privacySettings);

    bool CanStartConversation(
        Guid chapterId,
        Member member,
        MemberSubscription? subscription,
        ChapterMembershipSettings? membershipSettings,
        ChapterPrivacySettings? privacySettings);

    bool CanViewEvent(
        Event @event,
        Member? member,
        MemberSubscription? subscription,
        ChapterMembershipSettings? membershipSettings,
        ChapterPrivacySettings? privacySettings);

    bool CanViewVenue(
        Venue venue,
        Member? member,
        MemberSubscription? subscription,
        ChapterMembershipSettings? membershipSettings,
        ChapterPrivacySettings? privacySettings);

    Task<bool> ChapterHasAccess(
        Chapter chapter,
        SiteFeatureType feature);

    bool ChapterHasAccess(
        SiteSubscription? ownerSubscription,
        SiteFeatureType feature);

    SubscriptionStatus GetSubscriptionStatus(
        Member? member,
        MemberSubscription? subscription,
        ChapterMembershipSettings? membershipSettings);

    bool IsAdmin(
        Member? member,
        Chapter chapter,
        IEnumerable<ChapterAdminMember> adminMembers);
}