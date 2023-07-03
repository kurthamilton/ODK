using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ODK.Core.Members;

namespace ODK.Services.Members
{
    public interface IMemberService
    {
        Task<ServiceResult> ConfirmEmailAddressUpdate(Guid memberId, string confirmationToken);
        
        Task<ServiceResult> CreateMember(Guid chapterId, CreateMemberProfile profile);

        Task DeleteMember(Guid memberId);

        Task<VersionedServiceResult<IReadOnlyCollection<Member>>> GetLatestMembers(long? currentVersion, Guid currentMemberId, Guid chapterId);

        Task<Member> GetMember(Guid memberId);

        Task<VersionedServiceResult<MemberImage>> GetMemberImage(long? currentVersion, Guid memberId, int? size);

        Task<VersionedServiceResult<MemberImage>> GetMemberImage(long? currentVersion, Guid memberId, int? width, int? height);

        Task<MemberProfile> GetMemberProfile(Guid currentMemberId, Guid memberId);

        Task<MemberProfile> GetMemberProfile(Member currentMember, Member member);

        Task<IReadOnlyCollection<MemberProperty>> GetMemberProperties(Guid memberId);

        Task<VersionedServiceResult<IReadOnlyCollection<Member>>> GetMembers(long? currentVersion, Guid currentMemberId, Guid chapterId);

        Task<IReadOnlyCollection<Member>> GetMembers(Member currentMember, Guid chapterId);

        Task<MemberSubscription> GetMemberSubscription(Guid memberId);

        Task<ServiceResult> PurchaseSubscription(Guid memberId, Guid chapterSubscriptionId, string cardToken);

        Task<MemberSubscription> PurchaseSubscriptionOld(Guid memberId, Guid chapterSubscriptionId, string cardToken);

        Task<ServiceResult> RequestMemberEmailAddressUpdate(Guid memberId, string newEmailAddress);

        Task<MemberImage> RotateMemberImage(Guid memberId, int degrees);

        Task UpdateMemberEmailOptIn(Guid memberId, bool optIn);

        Task<MemberImage> UpdateMemberImage(Guid id, UpdateMemberImage image);

        Task<MemberProfile> UpdateMemberProfile(Guid id, UpdateMemberProfile profile);
    }
}
