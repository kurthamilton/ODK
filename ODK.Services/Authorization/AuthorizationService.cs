using ODK.Core.Chapters;
using ODK.Core.Events;
using ODK.Core.Features;
using ODK.Core.Members;
using ODK.Core.Venues;
using ODK.Data.Core;

namespace ODK.Services.Authorization;

public class AuthorizationService : IAuthorizationService
{
    private readonly IUnitOfWork _unitOfWork;

    public AuthorizationService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public bool CanRespondToEvent(
        Event @event,
        Member? member,
        MemberSubscription? subscription,
        ChapterMembershipSettings? membershipSettings,
        ChapterPrivacySettings? privacySettings)
    {        
        var memberVisibility = GetMemberVisibilityType(@event.ChapterId, member, subscription, membershipSettings);
        var responseVisibility = privacySettings?.EventResponseVisibility ?? ChapterFeatureVisibilityType.AllMembers;
        return memberVisibility.CanView(responseVisibility);
    }

    public bool CanStartConversation(
        Guid chapterId,
        Member member,
        MemberSubscription? subscription,
        ChapterMembershipSettings? membershipSettings,
        ChapterPrivacySettings? privacySettings)
    {
        var memberVisibility = GetMemberVisibilityType(chapterId, member, subscription, membershipSettings);
        var conversationVisibility = privacySettings?.Conversations ?? ChapterFeatureVisibilityType.Public;
        return memberVisibility.CanView(conversationVisibility);
    }

    public bool CanViewEvent(
        Event @event,
        Member? member,
        MemberSubscription? subscription,
        ChapterMembershipSettings? membershipSettings,
        ChapterPrivacySettings? privacySettings)
    {
        if (!@event.IsPublished)
        {
            return false;
        }

        var memberVisibility = GetMemberVisibilityType(@event.ChapterId, member, subscription, membershipSettings);
        var eventVisibility = privacySettings?.EventVisibility ?? ChapterFeatureVisibilityType.ActiveMembers;
        return memberVisibility.CanView(eventVisibility);
    }

    public bool CanViewVenue(
        Venue venue,
        Member? member,
        MemberSubscription? subscription,
        ChapterMembershipSettings? membershipSettings,
        ChapterPrivacySettings? privacySettings)
    {
        var memberVisibility = GetMemberVisibilityType(venue.ChapterId, member, subscription, membershipSettings);
        var venueVisibility = privacySettings?.VenueVisibility ?? ChapterFeatureVisibilityType.ActiveMembers;
        return memberVisibility.CanView(venueVisibility);
    }

    public async Task<bool> ChapterHasAccess(
        Chapter chapter,
        SiteFeatureType feature)
    {
        var ownerSubscription = await _unitOfWork.MemberSiteSubscriptionRepository.GetByChapterId(chapter.Id).Run();

        return ChapterHasAccess(ownerSubscription, feature);
    }

    public bool ChapterHasAccess(
        MemberSiteSubscription? ownerSubscription,
        SiteFeatureType feature) => ownerSubscription?.HasFeature(feature) == true;

    public SubscriptionStatus GetSubscriptionStatus(
        Member? member,
        MemberSubscription? subscription, 
        ChapterMembershipSettings? membershipSettings)
    {
        if (member == null || !member.IsCurrent())
        {
            return SubscriptionStatus.Disabled;
        }

        if (membershipSettings?.Enabled != true)
        {
            return SubscriptionStatus.Current;
        }

        if (subscription == null)
        {
            return SubscriptionStatus.Disabled;
        }

        if (subscription.ExpiresUtc == null || 
            membershipSettings.MembershipDisabledAfterDaysExpired <= 0)
        {
            return SubscriptionStatus.Current;
        }

        if (subscription.ExpiresUtc >= DateTime.UtcNow)
        {
            return subscription.ExpiresUtc >= DateTime.UtcNow.AddDays(membershipSettings.MembershipExpiringWarningDays)
                ? SubscriptionStatus.Current
                : SubscriptionStatus.Expiring;
        }

        return subscription.ExpiresUtc >= DateTime.UtcNow.AddDays(-1 * membershipSettings.MembershipDisabledAfterDaysExpired)
            ? SubscriptionStatus.Expired
            : SubscriptionStatus.Disabled;
    }

    private ChapterFeatureVisibilityType GetMemberVisibilityType(
        Guid chapterId,
        Member? member,
        MemberSubscription? subscription,
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
