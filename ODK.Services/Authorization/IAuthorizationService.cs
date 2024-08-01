using ODK.Core.Chapters;
using ODK.Core.Members;

namespace ODK.Services.Authorization;

public interface IAuthorizationService
{
    Task AssertMemberIsChapterMemberAsync(Guid memberId, Guid chapterId);

    void AssertMemberIsChapterMember(Member member, Guid chapterId);

    Task AssertMemberIsCurrent(Guid memberId);

    void AssertMemberIsCurrent(Member member);

    Task AssertMembershipIsActiveAsync(Guid memberId, Guid chapterId);

    string? GetRestrictedContentMessage(Member? member, Chapter? chapter, MemberSubscription? subscription, 
        ChapterMembershipSettings? membershipSettings);

    SubscriptionStatus GetSubscriptionStatus(MemberSubscription subscription, ChapterMembershipSettings membershipSettings);

    Task<bool> MembershipIsActiveAsync(Guid memberId, Guid chapterId);

    Task<bool> MembershipIsActiveAsync(MemberSubscription subscription, Guid chapterId);

    bool MembershipIsActive(MemberSubscription subscription, ChapterMembershipSettings membershipSettings);    
}
