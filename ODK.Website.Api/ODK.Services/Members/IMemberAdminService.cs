using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ODK.Core.Members;

namespace ODK.Services.Members
{
    public interface IMemberAdminService
    {
        Task AddMemberToGroup(Guid currentMemberId, Guid memberId, Guid memberGroupId);

        Task<MemberGroup> CreateMemberGroup(Guid currentMemberId, CreateMemberGroup memberGroup);

        Task DeleteMemberGroup(Guid currentMemberId, Guid id);

        Task DisableMember(Guid currentMemberId, Guid id);

        Task EnableMember(Guid currentMemberId, Guid id);

        Task<IReadOnlyCollection<MemberGroupMember>> GetMemberGroupMembers(Guid currentMemberId, Guid chapterId);

        Task<IReadOnlyCollection<MemberGroup>> GetMemberGroups(Guid currentMemberId, Guid chapterId);

        Task<IReadOnlyCollection<Member>> GetMembers(Guid currentMemberId, Guid chapterId);

        Task<IReadOnlyCollection<MemberSubscription>> GetMemberSubscriptions(Guid currentMemberId, Guid chapterId);

        Task RemoveMemberFromGroup(Guid currentMemberId, Guid memberId, Guid memberGroupId);

        Task<MemberImage> RotateMemberImage(Guid currentMemberId, Guid id, int degrees);

        Task<MemberGroup> UpdateMemberGroup(Guid currentMemberId, Guid id, CreateMemberGroup memberGroup);

        Task<MemberImage> UpdateMemberImage(Guid currentMemberId, Guid id, UpdateMemberImage image);
    }
}
