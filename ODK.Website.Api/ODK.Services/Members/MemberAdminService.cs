using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Services.Exceptions;

namespace ODK.Services.Members
{
    public class MemberAdminService : OdkAdminServiceBase, IMemberAdminService
    {
        private readonly IMemberGroupRepository _memberGroupRepository;
        private readonly IMemberRepository _memberRepository;
        private readonly IMemberService _memberService;

        public MemberAdminService(IChapterRepository chapterRepository, IMemberRepository memberRepository,
            IMemberGroupRepository memberGroupRepository, IMemberService memberService)
            : base(chapterRepository)
        {
            _memberGroupRepository = memberGroupRepository;
            _memberRepository = memberRepository;
            _memberService = memberService;
        }

        public async Task AddMemberToGroup(Guid currentMemberId, Guid memberId, Guid memberGroupId)
        {
            MemberGroup memberGroup = await GetMemberGroup(currentMemberId, memberGroupId);
            Member member = await _memberRepository.GetMember(memberId);
            if (member == null)
            {
                throw new OdkNotFoundException();
            }

            try
            {
                await _memberGroupRepository.AddMemberToGroup(member.Id, memberGroup.Id);
            }
            catch
            {
                throw new OdkServiceException("Member could not be added to group or is already in group");
            }
        }

        public async Task<MemberGroup> CreateMemberGroup(Guid currentMemberId, CreateMemberGroup memberGroup)
        {
            await AssertMemberIsChapterAdmin(currentMemberId, memberGroup.ChapterId);

            MemberGroup create = new MemberGroup(Guid.Empty, memberGroup.ChapterId, memberGroup.Name);

            await ValidateMemberGroup(create);

            return await _memberGroupRepository.CreateMemberGroup(create);
        }

        public async Task DeleteMemberGroup(Guid currentMemberId, Guid id)
        {
            MemberGroup memberGroup = await GetMemberGroup(currentMemberId, id);

            await _memberGroupRepository.DeleteMemberGroup(memberGroup.Id);
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

        public async Task<IReadOnlyCollection<MemberGroupMember>> GetMemberGroupMembers(Guid currentMemberId, Guid chapterId)
        {
            await AssertMemberIsChapterAdmin(currentMemberId, chapterId);

            return await _memberGroupRepository.GetMemberGroupMembers(chapterId);
        }

        public async Task<IReadOnlyCollection<MemberGroup>> GetMemberGroups(Guid currentMemberId, Guid chapterId)
        {
            await AssertMemberIsChapterAdmin(currentMemberId, chapterId);

            return await _memberGroupRepository.GetMemberGroups(chapterId);
        }

        public async Task<IReadOnlyCollection<Member>> GetMembers(Guid currentMemberId, Guid chapterId)
        {
            await AssertMemberIsChapterAdmin(currentMemberId, chapterId);

            return await _memberRepository.GetMembers(chapterId);
        }

        public async Task<IReadOnlyCollection<MemberSubscription>> GetMemberSubscriptions(Guid currentMemberId, Guid chapterId)
        {
            await AssertMemberIsChapterAdmin(currentMemberId, chapterId);

            return await _memberRepository.GetMemberSubscriptions(chapterId);
        }

        public async Task RemoveMemberFromGroup(Guid currentMemberId, Guid memberId, Guid memberGroupId)
        {
            MemberGroup memberGroup = await GetMemberGroup(currentMemberId, memberGroupId);

            await _memberGroupRepository.RemoveMemberFromGroup(memberId, memberGroup.Id);
        }

        public async Task<MemberImage> RotateMemberImage(Guid currentMemberId, Guid id, int degrees)
        {
            Member member = await GetMember(currentMemberId, id);

            return await _memberService.RotateMemberImage(member.Id, degrees);
        }

        public async Task<MemberGroup> UpdateMemberGroup(Guid currentMemberId, Guid id, CreateMemberGroup memberGroup)
        {
            MemberGroup update = await GetMemberGroup(currentMemberId, id);

            update.Update(memberGroup.Name);

            await ValidateMemberGroup(update);

            await _memberGroupRepository.UpdateMemberGroup(update);

            return update;
        }

        public async Task<MemberImage> UpdateMemberImage(Guid currentMemberId, Guid id, UpdateMemberImage image)
        {
            Member member = await GetMember(currentMemberId, id, true);

            return await _memberService.UpdateMemberImage(member.Id, image);
        }

        private async Task<Member> GetMember(Guid currentMemberId, Guid id, bool superAdmin = false)
        {
            Member member = await _memberRepository.GetMember(id);
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

        private async Task<MemberGroup> GetMemberGroup(Guid currentMemberId, Guid id)
        {
            MemberGroup memberGroup = await _memberGroupRepository.GetMemberGroup(id);
            if (memberGroup == null)
            {
                throw new OdkNotFoundException();
            }

            await AssertMemberIsChapterAdmin(currentMemberId, memberGroup.ChapterId);

            return memberGroup;
        }

        private async Task ValidateMemberGroup(MemberGroup memberGroup)
        {
            if (string.IsNullOrWhiteSpace(memberGroup.Name))
            {
                throw new OdkServiceException("Name is required");
            }

            IReadOnlyCollection<MemberGroup> existing = await _memberGroupRepository.GetMemberGroups(memberGroup.ChapterId);
            if (existing.Any(x => x.Id != memberGroup.Id && string.Equals(memberGroup.Name, x.Name, StringComparison.OrdinalIgnoreCase)))
            {
                throw new OdkServiceException("Name already exists");
            }
        }
    }
}
