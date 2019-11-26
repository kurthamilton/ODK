using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ODK.Core.Members
{
    public interface IMemberGroupRepository
    {
        Task AddMemberToGroup(Guid memberId, Guid memberGroupId);
        Task<MemberGroup> CreateMemberGroup(MemberGroup memberGroup);
        Task DeleteMemberGroup(Guid id);
        Task<MemberGroup> GetMemberGroup(Guid id);
        Task<IReadOnlyCollection<MemberGroupMember>> GetMemberGroupMembers(Guid chapterId);
        Task<IReadOnlyCollection<MemberGroup>> GetMemberGroups(Guid chapterId);
        Task RemoveMemberFromGroup(Guid memberId, Guid memberGroupId);
        Task UpdateMemberGroup(MemberGroup memberGroup);
    }
}
