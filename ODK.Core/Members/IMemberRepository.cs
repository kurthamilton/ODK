using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ODK.Core.Members
{
    public interface IMemberRepository
    {
        Task AddMemberGroup(Guid chapterId, string name);
        Task AddMemberToGroup(Guid memberId, Guid memberGroupId);
        Task AddPasswordResetRequest(Guid memberId, DateTime created, DateTime expires, string token);
        Task AddRefreshToken(Guid memberId, string refreshToken, DateTime expires);
        Task DeleteMemberGroup(Guid id);
        Task DeletePasswordResetRequest(Guid passwordResetRequestId);
        Task DeleteRefreshToken(Guid id);
        Task<Member> FindMemberByEmailAddress(string emailAddress);
        Task<Member> GetMember(Guid memberId);
        Task<IReadOnlyCollection<MemberGroup>> GetMemberGroups(Guid chapterId);
        Task<IReadOnlyCollection<MemberGroup>> GetMemberGroupsForMember(Guid memberId);
        Task<MemberImage> GetMemberImage(Guid memberId);
        Task<MemberPassword> GetMemberPassword(Guid memberId);
        Task<IReadOnlyCollection<MemberProperty>> GetMemberProperties(Guid memberId);
        Task<IReadOnlyCollection<Member>> GetMembers(Guid chapterId);
        Task<MemberPasswordResetRequest> GetPasswordResetRequest(string token);
        Task<MemberRefreshToken> GetRefreshToken(string refreshToken);
        Task RemoveMemberFromGroup(Guid memberId, Guid memberGroupId);
        Task UpdateMember(Guid memberId, string emailAddress, bool emailOptIn, string firstName, string lastName);
        Task UpdateMemberGroup(MemberGroup memberGroup);
        Task UpdateMemberImage(MemberImage image);
        Task UpdateMemberPassword(MemberPassword password);
        Task UpdateMemberProperties(Guid memberId, IEnumerable<MemberProperty> memberProperties);
    }
}