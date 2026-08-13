using ODK.Core.Chapters;
using ODK.Core.Events;
using ODK.Core.Features;
using ODK.Core.Members;
using ODK.Core.Subscriptions;
using ODK.Core.Venues;

namespace ODK.Services.Authorization;

public class AuthorizationService : IAuthorizationService
{
    public bool CanRespondToEvent(
        Event @event,
        Member? member,
        MemberChapterSubscription? subscription,
        ChapterMembershipSettings? membershipSettings,
        ChapterPrivacySettings? privacySettings)
    {
        var memberVisibility = GetMemberVisibilityType(@event.ChapterId, member, subscription, membershipSettings);
        var chapterVisibility = privacySettings.Visibility(ChapterFeatureType.EventResponses);
        return memberVisibility.CanView(chapterVisibility);
    }

    public bool CanViewEvent(
        Event @event,
        Member? member,
        MemberChapterSubscription? subscription,
        ChapterMembershipSettings? membershipSettings,
        ChapterPrivacySettings? privacySettings)
    {
        if (!@event.IsPublished)
        {
            return false;
        }

        var memberVisibility = GetMemberVisibilityType(@event.ChapterId, member, subscription, membershipSettings);
        var chapterVisibility = privacySettings.Visibility(ChapterFeatureType.Events);
        return memberVisibility.CanView(chapterVisibility);
    }

    public bool CanViewVenue(
        Venue venue,
        Member? member,
        MemberChapterSubscription? subscription,
        ChapterMembershipSettings? membershipSettings,
        ChapterPrivacySettings? privacySettings)
    {
        var memberVisibility = GetMemberVisibilityType(venue.ChapterId, member, subscription, membershipSettings);
        var chapterVisibility = privacySettings.Visibility(ChapterFeatureType.Venues);
        return memberVisibility.CanView(chapterVisibility);
    }

    public bool ChapterHasAccess(
        IEnumerable<SiteSubscriptionFeature> ownerSubscriptionFeatures,
        SiteFeatureType feature)
        => ownerSubscriptionFeatures.Any(x => x.Feature == feature);

    public SubscriptionStatus GetSubscriptionStatus(
        Member? member,
        MemberChapterSubscription? subscription,
        ChapterMembershipSettings? membershipSettings)
    {
        if (member == null || !member.IsCurrent())
        {
            return SubscriptionStatus.Unactivated;
        }

        if (membershipSettings?.Enabled != true)
        {
            return SubscriptionStatus.Current;
        }

        if (subscription == null)
        {
            return SubscriptionStatus.Disabled;
        }

        if (subscription.ExpiresUtc == null)
        {
            return SubscriptionStatus.Current;
        }

        if (subscription.ExpiresUtc >= DateTime.UtcNow)
        {
            return subscription.ExpiresUtc >= DateTime.UtcNow.AddDays(membershipSettings.MembershipExpiringWarningDays)
                ? SubscriptionStatus.Current
                : SubscriptionStatus.Expiring;
        }

        // The cooldown is how long an expired membership keeps its access. None means access ends with the
        // subscription; a negative is meaningless and is treated the same way. A membership that never ends
        // is not expressible here - it has no expiry date at all, handled above.
        var cooldownDays = Math.Max(0, membershipSettings.MembershipDisabledAfterDaysExpired);

        return subscription.ExpiresUtc >= DateTime.UtcNow.AddDays(-cooldownDays)
            ? SubscriptionStatus.Expired
            : SubscriptionStatus.Disabled;
    }

    private ChapterFeatureVisibilityType GetMemberVisibilityType(
        Guid chapterId,
        Member? member,
        MemberChapterSubscription? subscription,
        ChapterMembershipSettings? membershipSettings)
    {
        var subscriptionStatus = member?.IsApprovedMemberOf(chapterId) == true
            ? GetSubscriptionStatus(member, subscription, membershipSettings)
            : SubscriptionStatus.None;

        switch (subscriptionStatus)
        {
            case SubscriptionStatus.Current:
            case SubscriptionStatus.Expiring:
                return ChapterFeatureVisibilityType.ActiveMembers;

            case SubscriptionStatus.Expired:
                return ChapterFeatureVisibilityType.AllMembers;

            default:
                return ChapterFeatureVisibilityType.Public;
        }
    }
}