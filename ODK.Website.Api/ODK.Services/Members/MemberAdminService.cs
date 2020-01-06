using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Services.Caching;
using ODK.Services.Exceptions;

namespace ODK.Services.Members
{
    public class MemberAdminService : OdkAdminServiceBase, IMemberAdminService
    {
        private readonly ICacheService _cacheService;
        private readonly IMemberRepository _memberRepository;
        private readonly IMemberService _memberService;

        public MemberAdminService(IChapterRepository chapterRepository, IMemberRepository memberRepository,
            IMemberService memberService, ICacheService cacheService)
            : base(chapterRepository)
        {
            _cacheService = cacheService;
            _memberRepository = memberRepository;
            _memberService = memberService;
        }

        public async Task DeleteMember(Guid currentMemberId, Guid memberId)
        {
            Member member = await GetMember(currentMemberId, memberId);

            await _memberRepository.DeleteMember(member.Id);

            _cacheService.RemoveVersionedCollection<Member>(member.ChapterId);
            _cacheService.RemoveVersionedItem<Member>(memberId);
        }

        public async Task DisableMember(Guid currentMemberId, Guid id)
        {
            Member member = await GetMember(currentMemberId, id);

            await _memberRepository.DisableMember(member.Id);
        }

        public async Task EnableMember(Guid currentMemberId, Guid id)
        {
            Member member = await GetMember(currentMemberId, id);

            await _memberRepository.EnableMember(member.Id);
        }

        public async Task<Member> GetMember(Guid currentMemberId, Guid memberId)
        {
            return await GetMember(currentMemberId, memberId, false);
        }

        public async Task<IReadOnlyCollection<Member>> GetMembers(Guid currentMemberId, Guid chapterId)
        {
            await AssertMemberIsChapterAdmin(currentMemberId, chapterId);

            return await _memberRepository.GetMembers(chapterId, true);
        }

        public async Task<MemberSubscription> GetMemberSubscription(Guid currentMemberId, Guid memberId)
        {
            Member member = await GetMember(currentMemberId, memberId);

            return await _memberRepository.GetMemberSubscription(member.Id);
        }

        public async Task<IReadOnlyCollection<MemberSubscription>> GetMemberSubscriptions(Guid currentMemberId, Guid chapterId)
        {
            await AssertMemberIsChapterAdmin(currentMemberId, chapterId);

            return await _memberRepository.GetMemberSubscriptions(chapterId);
        }

        public async Task<MemberImage> RotateMemberImage(Guid currentMemberId, Guid memberId, int degrees)
        {
            Member member = await GetMember(currentMemberId, memberId);

            MemberImage rotated = await _memberService.RotateMemberImage(member.Id, degrees);

            _cacheService.RemoveVersionedItem<MemberImage>(memberId);

            return rotated;
        }

        public async Task<MemberImage> UpdateMemberImage(Guid currentMemberId, Guid memberId, UpdateMemberImage image)
        {
            Member member = await GetMember(currentMemberId, memberId, true);

            MemberImage updated = await _memberService.UpdateMemberImage(member.Id, image);

            _cacheService.RemoveVersionedItem<MemberImage>(memberId);

            return updated;
        }

        public async Task<MemberSubscription> UpdateMemberSubscription(Guid currentMemberId, Guid memberId,
            UpdateMemberSubscription subscription)
        {
            Member member = await GetMember(currentMemberId, memberId);

            DateTime? expiryDate = subscription.Type == SubscriptionType.Alum ? new DateTime?() : subscription.ExpiryDate;

            MemberSubscription update = new MemberSubscription(member.Id, subscription.Type, expiryDate);

            ValidateMemberSubscription(update);

            await _memberRepository.UpdateMemberSubscription(update);

            _cacheService.RemoveVersionedItem<MemberSubscription>(memberId);
            _cacheService.RemoveVersionedCollection<Member>(member.ChapterId);

            return update;
        }

        private async Task<Member> GetMember(Guid currentMemberId, Guid id, bool superAdmin)
        {
            Member member = await _memberRepository.GetMember(id, true);
            if (member == null)
            {
                throw new OdkNotFoundException();
            }

            if (superAdmin)
            {
                await AssertMemberIsChapterSuperAdmin(currentMemberId, member.ChapterId);
            }
            else
            {
                await AssertMemberIsChapterAdmin(currentMemberId, member.ChapterId);
            }

            return member;
        }

        private void ValidateMemberSubscription(MemberSubscription subscription)
        {
            if (!Enum.IsDefined(typeof(SubscriptionType), subscription.Type) || subscription.Type == SubscriptionType.None)
            {
                throw new OdkServiceException("Invalid type");
            }

            if (subscription.Type == SubscriptionType.Alum && subscription.ExpiryDate != null)
            {
                throw new OdkServiceException("Alum should not have expiry date");
            }
            else if (subscription.Type != SubscriptionType.Alum && subscription.ExpiryDate < DateTime.UtcNow.Date)
            {
                throw new OdkServiceException("Expiry date should not be in the past");
            }
        }
    }
}
