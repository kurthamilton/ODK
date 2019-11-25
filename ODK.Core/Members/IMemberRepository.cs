using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ODK.Core.Members
{
    public interface IMemberRepository
    {
        Task AddMemberGroup(int chapterId, string name);
        Task AddMemberToGroup(int memberId, int groupId);
        Task AddPasswordResetRequest(Guid memberId, DateTime created, DateTime expires, string token);
        Task AddRefreshToken(Guid memberId, string refreshToken, DateTime expires);
        Task DeleteMemberGroup(int groupId);
        Task DeletePasswordResetRequest(Guid passwordResetRequestId);
        Task DeleteRefreshToken(Guid id);
        Task<Member> FindMemberByEmailAddress(string emailAddress);
        Task<Member> GetMember(Guid memberId);
        Task<Dictionary<int, IReadOnlyCollection<MemberGroup>>> GetMemberGroupMembers(int chapterId);
        Task<IReadOnlyCollection<MemberGroup>> GetMemberGroups(int chapterId);
        Task<IReadOnlyCollection<MemberGroup>> GetMemberGroupsForMember(int memberId);
        Task<MemberImage> GetMemberImage(Guid memberId);
        Task<MemberPassword> GetMemberPassword(Guid memberId);
        Task<IReadOnlyCollection<MemberProperty>> GetMemberProperties(Guid memberId);
        Task<IReadOnlyCollection<Member>> GetMembers(Guid chapterId);
        Task<MemberPasswordResetRequest> GetPasswordResetRequest(string token);
        Task<MemberRefreshToken> GetRefreshToken(string refreshToken);
        Task RemoveMemberFromGroup(int memberId, int groupId);
        Task UpdateMember(Guid memberId, string emailAddress, bool emailOptIn, string firstName, string lastName);
        Task UpdateMemberGroup(int chapterId, string oldName, string newName);
        Task UpdateMemberImage(MemberImage image);
        Task UpdateMemberPassword(MemberPassword password);
        Task UpdateMemberProperties(Guid memberId, IEnumerable<MemberProperty> memberProperties);
    }
}