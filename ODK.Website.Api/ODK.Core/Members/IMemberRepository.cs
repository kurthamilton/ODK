using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ODK.Core.Members
{
    public interface IMemberRepository
    {
        Task ActivateMember(Guid memberId);
        Task AddActivationToken(MemberActivationToken token);
        Task AddMemberImage(MemberImage image);
        Task AddPasswordResetRequest(Guid memberId, DateTime created, DateTime expires, string token);
        Task AddRefreshToken(MemberRefreshToken token);
        Task<Guid> CreateMember(Member member);
        Task DeleteActivationToken(Guid memberId);
        Task DeletePasswordResetRequest(Guid passwordResetRequestId);
        Task DeleteRefreshToken(MemberRefreshToken refreshToken);
        Task DisableMember(Guid id);
        Task EnableMember(Guid id);
        Task<Member> FindMemberByEmailAddress(string emailAddress);
        Task<IReadOnlyCollection<Member>> GetLatestMembers(Guid chapterId, int maxSize);
        Task<Member> GetMember(Guid memberId);
        Task<MemberActivationToken> GetMemberActivationToken(Guid memberId);
        Task<MemberActivationToken> GetMemberActivationToken(string activationToken);
        Task<MemberImage> GetMemberImage(Guid memberId, long? versionAfter);
        Task<MemberPassword> GetMemberPassword(Guid memberId);
        Task<IReadOnlyCollection<MemberProperty>> GetMemberProperties(Guid memberId);
        Task<IReadOnlyCollection<Member>> GetMembers(Guid chapterId);
        Task<MemberSubscription> GetMemberSubscription(Guid memberId);
        Task<MemberPasswordResetRequest> GetPasswordResetRequest(string token);
        Task<MemberRefreshToken> GetRefreshToken(string refreshToken);
        Task UpdateMember(Guid memberId, string emailAddress, bool emailOptIn, string firstName, string lastName);
        Task UpdateMemberImage(MemberImage image);
        Task UpdateMemberPassword(MemberPassword password);
        Task UpdateMemberProperties(Guid memberId, IEnumerable<MemberProperty> memberProperties);
        Task UpdateMemberSubscription(MemberSubscription memberSubscription);
    }
}