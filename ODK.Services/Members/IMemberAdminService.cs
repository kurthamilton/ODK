using ODK.Core.Members;

namespace ODK.Services.Members;

public interface IMemberAdminService
{
    Task DeleteMember(AdminServiceRequest request, Guid memberId);

    Task<Member> GetMember(AdminServiceRequest request, Guid memberId);

    Task<MemberAvatar?> GetMemberAvatar(AdminServiceRequest request, Guid memberId);

    Task<IReadOnlyCollection<IReadOnlyCollection<string>>> GetMemberCsv(AdminServiceRequest request);

    Task<MemberImage?> GetMemberImage(AdminServiceRequest request, Guid memberId);

    Task<IReadOnlyCollection<Member>> GetMembers(AdminServiceRequest request);

    Task<IReadOnlyCollection<Member>> GetMembers(AdminServiceRequest request, MemberFilter filter);

    Task<MembersDto> GetMembersDto(AdminServiceRequest request);

    Task<MemberSubscription?> GetMemberSubscription(AdminServiceRequest request, Guid memberId);

    Task RotateMemberImage(AdminServiceRequest request, Guid memberId);

    Task SendActivationEmail(AdminServiceRequest request, Guid memberId);

    Task<ServiceResult> SendBulkEmail(AdminServiceRequest request, MemberFilter filter, string subject, string body);

    Task<ServiceResult> SendMemberEmail(AdminServiceRequest request, Guid memberId, string subject, string body);

    Task SendMemberSubscriptionReminderEmails();

    Task SetMemberVisibility(AdminServiceRequest request, Guid memberId, bool visible);

    Task<ServiceResult> UpdateMemberImage(AdminServiceRequest request, Guid id,
        UpdateMemberImage? model, MemberImageCropInfo cropInfo);

    Task<ServiceResult> UpdateMemberSubscription(AdminServiceRequest request, Guid memberId, 
        UpdateMemberSubscription subscription);
}
