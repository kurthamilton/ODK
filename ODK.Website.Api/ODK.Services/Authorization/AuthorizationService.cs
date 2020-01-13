using System;
using System.Threading.Tasks;
using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Services.Caching;
using ODK.Services.Exceptions;

namespace ODK.Services.Authorization
{
    public class AuthorizationService : IAuthorizationService
    {
        private readonly ICacheService _cacheService;
        private readonly IChapterRepository _chapterRepository;
        private readonly IMemberRepository _memberRepository;

        public AuthorizationService(IMemberRepository memberRepository, ICacheService cacheService,
            IChapterRepository chapterRepository)
        {
            _cacheService = cacheService;
            _chapterRepository = chapterRepository;
            _memberRepository = memberRepository;
        }

        public async Task AssertMemberIsChapterMember(Guid memberId, Guid chapterId)
        {
            Member member = await GetMember(memberId);
            AssertMemberIsChapterMember(member, chapterId);
        }

        public void AssertMemberIsChapterMember(Member member, Guid chapterId)
        {
            AssertMemberIsCurrent(member);
            if (member.ChapterId != chapterId)
            {
                throw new OdkNotAuthorizedException();
            }
        }

        public async Task AssertMemberIsCurrent(Guid memberId)
        {
            Member member = await GetMember(memberId);
            AssertMemberIsCurrent(member);
        }

        public void AssertMemberIsCurrent(Member member)
        {
            if (member == null || !member.Activated || member.Disabled)
            {
                throw new OdkNotFoundException();
            }
        }

        public async Task AssertMembershipIsActive(Guid memberId, Guid chapterId)
        {
            MemberSubscription subscription = await GetMemberSubscription(memberId);
            if (!await MembershipIsActive(subscription, chapterId))
            {
                throw new OdkNotAuthorizedException();
            }
        }

        public async Task<bool> MembershipIsActive(MemberSubscription subscription, Guid chapterId)
        {
            ChapterMembershipSettings membershipSettings = await _cacheService.GetOrSetItem(
                () => _chapterRepository.GetChapterMembershipSettings(chapterId),
                chapterId);

            return MembershipIsActive(subscription, membershipSettings);
        }

        public bool MembershipIsActive(MemberSubscription subscription, ChapterMembershipSettings membershipSettings)
        {
            if (subscription.Type == SubscriptionType.Alum)
            {
                return false;
            }

            if (subscription.ExpiryDate == null || subscription.ExpiryDate >= DateTime.UtcNow)
            {
                return true;
            }

            if (membershipSettings.MembershipDisabledAfterDaysExpired <= 0)
            {
                return true;
            }

            return subscription.ExpiryDate >= DateTime.UtcNow.AddDays(-1 * membershipSettings.MembershipDisabledAfterDaysExpired);
        }

        private async Task<Member> GetMember(Guid id)
        {
            VersionedServiceResult<Member> member = await _cacheService.GetOrSetVersionedItem(
                () => _memberRepository.GetMember(id, true), id, null);
            AssertMemberIsCurrent(member.Value);
            return member.Value;
        }

        private async Task<MemberSubscription> GetMemberSubscription(Guid memberId)
        {
            VersionedServiceResult<MemberSubscription> subscription = await _cacheService.GetOrSetVersionedItem(
                () => _memberRepository.GetMemberSubscription(memberId), memberId, null);
            return subscription.Value;
        }
    }
}
