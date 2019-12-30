using System;
using System.Threading.Tasks;
using ODK.Core.Chapters;
using ODK.Core.Members;

namespace ODK.Services.Authorization
{
    public interface IAuthorizationService
    {
        Task AssertMemberIsChapterMember(Guid memberId, Guid chapterId);

        void AssertMemberIsChapterMember(Member member, Guid chapterId);

        Task AssertMemberIsCurrent(Guid memberId);

        void AssertMemberIsCurrent(Member member);

        Task AssertMembershipIsActive(Guid memberId, Guid chapterId);

        Task<bool> MembershipIsActive(MemberSubscription subscription, Guid chapterId);

        bool MembershipIsActive(MemberSubscription subscription, ChapterMembershipSettings membershipSettings);
    }
}
