using System;
using System.Threading.Tasks;
using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Services.Exceptions;

namespace ODK.Services.Authorization
{
    public class AuthorizationService : IAuthorizationService
    {
        private readonly IChapterRepository _chapterRepository;
        private readonly IMemberRepository _memberRepository;

        public AuthorizationService(IMemberRepository memberRepository, 
            IChapterRepository chapterRepository)
        {
            _chapterRepository = chapterRepository;
            _memberRepository = memberRepository;
        }

        public async Task AssertMemberIsChapterMember(Guid memberId, Guid chapterId)
        {
            Member member = await GetMember(memberId);
            await AssertMemberIsChapterMember(member, chapterId);
        }

        public async Task AssertMemberIsChapterMember(Member member, Guid chapterId)
        {
            AssertMemberIsCurrent(member);
            if (member.ChapterId == chapterId)
            {
                return;
            }

            ChapterAdminMember? chapterAdminMember = await _chapterRepository.GetChapterAdminMember(chapterId, member.Id);
            if (chapterAdminMember != null)
            {
                return;
            }

            throw new OdkNotAuthorizedException();
        }

        public async Task AssertMemberIsCurrent(Guid memberId)
        {
            Member member = await GetMember(memberId);
            AssertMemberIsCurrent(member);
        }

        public void AssertMemberIsCurrent(Member? member)
        {
            if (member == null || !member.Activated || member.Disabled)
            {
                throw new OdkNotFoundException();
            }
        }

        public async Task AssertMembershipIsActive(Guid memberId, Guid chapterId)
        {
            MemberSubscription? subscription = await GetMemberSubscription(memberId);
            if (subscription == null || !await MembershipIsActive(subscription, chapterId))
            {
                throw new OdkNotAuthorizedException();
            }
        }

        public async Task<bool> MembershipIsActive(MemberSubscription subscription, Guid chapterId)
        {
            ChapterMembershipSettings? membershipSettings = await _chapterRepository.GetChapterMembershipSettings(chapterId);

            return MembershipIsActive(subscription, membershipSettings);
        }

        public bool MembershipIsActive(MemberSubscription subscription, ChapterMembershipSettings? membershipSettings)
        {
            if (subscription.Type == SubscriptionType.Alum)
            {
                return false;
            }

            if (subscription.ExpiryDate == null || subscription.ExpiryDate >= DateTime.UtcNow)
            {
                return true;
            }

            if (membershipSettings == null)
            {
                return false;
            }

            if (membershipSettings.MembershipDisabledAfterDaysExpired <= 0)
            {
                return true;
            }

            return subscription.ExpiryDate >= DateTime.UtcNow.AddDays(-1 * membershipSettings.MembershipDisabledAfterDaysExpired);
        }

        private async Task<Member> GetMember(Guid id)
        {
            Member? member = await _memberRepository.GetMember(id, true);
            AssertMemberIsCurrent(member);
            return member!;
        }

        private async Task<MemberSubscription?> GetMemberSubscription(Guid memberId)
        {
            return await _memberRepository.GetMemberSubscription(memberId);
        }
    }
}
