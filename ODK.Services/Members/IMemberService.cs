using ODK.Core.Chapters;
using ODK.Core.Countries;
using ODK.Core.Members;

namespace ODK.Services.Members;

public interface IMemberService
{
    Task<ServiceResult> ConfirmEmailAddressUpdate(Guid memberId, string confirmationToken);

    Task<ServiceResult> CreateAccount(CreateAccountModel model);

    Task<ServiceResult> CreateMember(Guid chapterId, CreateMemberProfile model);

    Task DeleteMember(Guid memberId);

    Task<Member> GetMember(Guid memberId);

    Task<Member> GetMember(Guid memberId, Guid chapterId);

    Task<VersionedServiceResult<MemberAvatar>> GetMemberAvatar(long? currentVersion, Guid memberId);

    Task<VersionedServiceResult<MemberImage>> GetMemberImage(long? currentVersion, Guid memberId);

    Task<MemberLocation?> GetMemberLocation(Guid memberId);

    Task<MemberProfile?> GetMemberProfile(Guid chapterId, Guid currentMemberId, Member member);

    Task<IReadOnlyCollection<Member>> GetMembers(Member? currentMember, Guid chapterId);

    Task<ServiceResult> PurchaseSubscription(Guid memberId, Guid chapterId, Guid chapterSubscriptionId, string cardToken);

    Task<ServiceResult> RequestMemberEmailAddressUpdate(Guid memberId, Guid chapterId, string newEmailAddress);

    Task<ServiceResult> RequestMemberEmailAddressUpdate(Guid memberId, string newEmailAddress);

    Task RotateMemberImage(Guid memberId);

    Task UpdateMemberEmailOptIn(Guid memberId, bool optIn);

    Task<ServiceResult> UpdateMemberImage(Guid id, UpdateMemberImage? model, MemberImageCropInfo cropInfo);    

    Task<ServiceResult> UpdateMemberChapterProfile(Guid id, Guid chapterId, UpdateMemberChapterProfile model);

    Task<ServiceResult> UpdateMemberLocation(Guid id, LatLong? location, string? name);

    Task<ServiceResult> UpdateMemberSiteProfile(Guid id, UpdateMemberSiteProfile model);
}
