using ODK.Core.Chapters;
using ODK.Core.Members;

namespace ODK.Services.Members;

public interface IMemberService
{
    Task<ServiceResult> ConfirmEmailAddressUpdate(Guid memberId, string confirmationToken);
    
    Task<ServiceResult> CreateMember(Guid chapterId, CreateMemberProfile model);

    Task DeleteMember(Guid memberId);

    Task<IReadOnlyCollection<Member>> GetLatestMembers(Member currentMember, Guid chapterId);

    Task<Member?> GetMember(Guid memberId, Guid chapterId);

    Task<VersionedServiceResult<MemberImage>> GetMemberImage(long? currentVersion, Guid memberId, int? size);

    Task<VersionedServiceResult<MemberImage>> GetMemberImage(long? currentVersion, Guid memberId, int? width, int? height);
    
    Task<MemberProfile?> GetMemberProfile(Guid chapterId, Member currentMember, Member? member);

    Task<IReadOnlyCollection<Member>> GetMembers(Member? currentMember, Guid chapterId);

    Task<ServiceResult> PurchaseSubscription(Guid memberId, Guid chapterId, Guid chapterSubscriptionId, string cardToken);

    Task<ServiceResult> RequestMemberEmailAddressUpdate(Guid memberId, Guid chapterId, string newEmailAddress);

    Task RotateMemberImage(Guid memberId, int degrees);

    Task SendActivationEmailAsync(Chapter chapter, Member member, string activationToken);

    Task UpdateMemberEmailOptIn(Guid memberId, bool optIn);

    Task<ServiceResult> UpdateMemberImage(Guid id, UpdateMemberImage model);

    Task<ServiceResult> UpdateMemberProfile(Guid id, Guid chapterId, UpdateMemberProfile model);
}
