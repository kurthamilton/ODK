using ODK.Core.Chapters;
using ODK.Core.Events;
using ODK.Core.Features;
using ODK.Core.Members;
using ODK.Core.Subscriptions;
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
        var chapterVisibility = privacySettings.Visibility(ChapterFeatureType.EventResponses);
        return memberVisibility.CanView(chapterVisibility);
    }

    public bool CanStartConversation(
        Guid chapterId,
        Member member,
        MemberSubscription? subscription,
        ChapterMembershipSettings? membershipSettings,
        ChapterPrivacySettings? privacySettings)
    {
        var memberVisibility = GetMemberVisibilityType(chapterId, member, subscription, membershipSettings);
        var chapterVisibility = privacySettings.Visibility(ChapterFeatureType.Conversations);
        return memberVisibility.CanView(chapterVisibility);
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
        var chapterVisibility = privacySettings.Visibility(ChapterFeatureType.Events);
        return memberVisibility.CanView(chapterVisibility);
    }

    public bool CanViewVenue(
        Venue venue,
        Member? member,
        MemberSubscription? subscription,
        ChapterMembershipSettings? membershipSettings,
        ChapterPrivacySettings? privacySettings)
    {
        var memberVisibility = GetMemberVisibilityType(venue.ChapterId, member, subscription, membershipSettings);
        var chapterVisibility = privacySettings.Visibility(ChapterFeatureType.Venues);
        return memberVisibility.CanView(chapterVisibility);
    }

    public async Task<bool> ChapterHasAccess(
        Chapter chapter,
        SiteFeatureType feature)
    {
        var dto = await _unitOfWork.MemberSiteSubscriptionRepository
            .GetDtoByChapterId(chapter.Id)
            .Run();

        return ChapterHasAccess(dto?.SiteSubscription, feature);
    }

    public bool ChapterHasAccess(
        SiteSubscription? ownerSubscription,
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

    public bool IsAdmin(
        Member? member,
        Chapter chapter,
        IEnumerable<ChapterAdminMember> adminMembers)
    {
        if (member == null)
        {
            return false;
        }

        return adminMembers
            .FirstOrDefault(x => x.MemberId == member.Id && x.ChapterId == chapter.Id)
            .HasAccessTo(ChapterAdminRole.Organiser, member);
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