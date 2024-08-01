using ODK.Core.Members;

namespace ODK.Services.Members;

public interface IMemberAdminService
{
    Task DeleteMember(Guid currentMemberId, Guid memberId);

    Task<Member> GetMember(Guid currentMemberId, Guid memberId);

    Task<IReadOnlyCollection<IReadOnlyCollection<string>>> GetMemberCsv(Guid currentMemberId, Guid chapterId);

    Task<IReadOnlyCollection<Member>> GetMembers(Guid currentMemberId, Guid chapterId);

    Task<IReadOnlyCollection<Member>> GetMembers(Guid currentMemberId, MemberFilter filter);

    Task<MembersDto> GetMembersDto(Guid currentMemberId, Guid chapterId);

    Task<MemberSubscription?> GetMemberSubscription(Guid currentMemberId, Guid chapterId, Guid memberId);

    Task<IReadOnlyCollection<MemberSubscription>> GetMemberSubscriptions(Guid currentMemberId, Guid chapterId);
    
    Task RotateMemberImage(Guid currentMemberId, Guid memberId, int degrees);

    Task SendActivationEmail(Guid currentMemberId, Guid chapterId, Guid memberId);

    Task<ServiceResult> UpdateMemberSubscription(Guid currentMemberId, Guid chapterId, Guid memberId, 
        UpdateMemberSubscription subscription);
}
