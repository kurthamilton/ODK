using ODK.Core.Members;

namespace ODK.Services.Members;

public interface IMemberAdminService
{
    Task DeleteMember(Guid currentMemberId, Guid memberId);

    Task<Member?> GetMember(Guid currentMemberId, Guid memberId);

    Task<IReadOnlyCollection<Member>> GetMembers(Guid currentMemberId, Guid chapterId, bool requireSuperAdmin = false);

    Task<IReadOnlyCollection<Member>> GetMembers(Guid currentMemberId, MemberFilter filter);

    Task<MembersDto> GetMembersDto(Guid currentMemberId, Guid chapterId);

    Task<MemberSubscription?> GetMemberSubscription(Guid currentMemberId, Guid memberId);

    Task<IReadOnlyCollection<MemberSubscription>> GetMemberSubscriptions(Guid currentMemberId, Guid chapterId);
    
    Task RotateMemberImage(Guid currentMemberId, Guid memberId, int degrees);
    
    Task<ServiceResult> UpdateMemberSubscription(Guid currentMemberId, Guid memberId, UpdateMemberSubscription subscription);
}
