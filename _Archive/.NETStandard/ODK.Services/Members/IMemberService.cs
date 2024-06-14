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

        Task<IReadOnlyCollection<Member>> GetLatestMembers(Member currentMember, Guid chapterId);

        Task<Member?> GetMember(Guid memberId);

        Task<VersionedServiceResult<MemberImage>> GetMemberImage(long? currentVersion, Guid memberId, int? size);

        Task<VersionedServiceResult<MemberImage>> GetMemberImage(long? currentVersion, Guid memberId, int? width, int? height);
        
        Task<MemberProfile?> GetMemberProfile(Member currentMember, Member? member);

        Task<IReadOnlyCollection<MemberProperty>> GetMemberProperties(Guid memberId);

        Task<IReadOnlyCollection<Member>> GetMembers(Member? currentMember, Guid chapterId);

        Task<MemberSubscription?> GetMemberSubscription(Guid memberId);

        Task<ServiceResult> PurchaseSubscription(Guid memberId, Guid chapterSubscriptionId, string cardToken);

        Task<ServiceResult> RequestMemberEmailAddressUpdate(Guid memberId, string newEmailAddress);

        Task RotateMemberImage(Guid memberId, int degrees);

        Task UpdateMemberEmailOptIn(Guid memberId, bool optIn);

        Task UpdateMemberImage(Guid id, UpdateMemberImage image);

        Task<ServiceResult> UpdateMemberProfile(Guid id, UpdateMemberProfile profile);
    }
}
