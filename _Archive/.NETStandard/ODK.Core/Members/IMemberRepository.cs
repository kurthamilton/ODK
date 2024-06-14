using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ODK.Core.Members
{
    public interface IMemberRepository
    {
        Task ActivateMember(Guid memberId);
        Task AddActivationToken(MemberActivationToken token);
        Task AddEmailAddressUpdateToken(MemberEmailAddressUpdateToken token);
        Task AddMemberImage(MemberImage image);
        Task AddMemberSubscriptionRecord(MemberSubscriptionRecord record);
        Task AddPasswordResetRequest(Guid memberId, DateTime created, DateTime expires, string token);
        Task<Guid> CreateMember(Member member);
        Task DeleteActivationToken(Guid memberId);
        Task DeleteEmailAddressUpdateToken(Guid memberId);
        Task DeleteMember(Guid memberId);
        Task DeletePasswordResetRequest(Guid passwordResetRequestId);
        Task DisableMember(Guid id);
        Task EnableMember(Guid id);
        Task<Member?> FindMemberByEmailAddress(string emailAddress);
        Task<Member?> GetMember(Guid memberId, bool searchAll = false);
        Task<MemberActivationToken?> GetMemberActivationToken(string activationToken);
        Task<MemberEmailAddressUpdateToken?> GetMemberEmailAddressUpdateToken(Guid memberId);
        Task<MemberImage?> GetMemberImage(Guid memberId);
        Task<MemberPassword?> GetMemberPassword(Guid memberId);
        Task<IReadOnlyCollection<MemberProperty>> GetMemberProperties(Guid memberId);
        Task<IReadOnlyCollection<Member>> GetMembers(Guid chapterId, bool searchAll = false);
        Task<MemberSubscription?> GetMemberSubscription(Guid memberId);
        Task<IReadOnlyCollection<MemberSubscription>> GetMemberSubscriptions(Guid chapterId);
        Task<long> GetMembersVersion(Guid chapterId, bool searchAll = false);
        Task<MemberPasswordResetRequest?> GetPasswordResetRequest(string token);
        Task UpdateMember(Guid memberId, bool emailOptIn, string firstName, string lastName);
        Task UpdateMemberEmailAddress(Guid memberId, string emailAddress);
        Task UpdateMemberImage(MemberImage image);
        Task UpdateMemberPassword(MemberPassword password);
        Task UpdateMemberProperties(Guid memberId, IEnumerable<MemberProperty> memberProperties);
        Task UpdateMemberSubscription(MemberSubscription memberSubscription);
    }
}