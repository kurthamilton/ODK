using ODK.Core.Chapters;
using ODK.Core.Members;

namespace ODK.Services.Authorization;

public interface IAuthorizationService
{
    void AssertMemberIsCurrent(Member member);

    SubscriptionStatus GetSubscriptionStatus(MemberSubscription subscription, ChapterMembershipSettings membershipSettings);

    Task<bool> MembershipIsActiveAsync(Guid memberId, Guid chapterId);

    Task<bool> MembershipIsActiveAsync(MemberSubscription subscription, Guid chapterId);

    bool MembershipIsActive(MemberSubscription subscription, ChapterMembershipSettings membershipSettings);    
}
