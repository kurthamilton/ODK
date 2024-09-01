using ODK.Core.Members;
using ODK.Services.Members.ViewModels;

namespace ODK.Services.Members;

public interface IMemberAdminService
{
    Task DeleteMember(AdminServiceRequest request, Guid memberId);

    Task<AdminMembersAdminPageViewModel> GetAdminMembersAdminPageViewModel(AdminServiceRequest request);

    Task<BulkEmailAdminPageViewModel> GetBulkEmailViewModel(AdminServiceRequest request);

    Task<Member> GetMember(AdminServiceRequest request, Guid memberId);

    Task<MemberAvatar?> GetMemberAvatar(AdminServiceRequest request, Guid memberId);

    Task<IReadOnlyCollection<IReadOnlyCollection<string>>> GetMemberCsv(AdminServiceRequest request);

    Task<MemberEmailAdminPageViewModel> GetMemberEmailViewModel(AdminServiceRequest request, Guid memberId);

    Task<MemberEventsAdminPageViewModel> GetMemberEventsViewModel(AdminServiceRequest request, Guid memberId);

    Task<MemberImage?> GetMemberImage(AdminServiceRequest request, Guid memberId);

    Task<SubscriptionCreateAdminPageViewModel> GetMemberSubscriptionCreateViewModel(AdminServiceRequest request);

    Task<SubscriptionsAdminPageViewModel> GetMemberSubscriptionsViewModel(AdminServiceRequest request);

    Task<SubscriptionAdminPageViewModel> GetMemberSubscriptionViewModel(AdminServiceRequest request, Guid subscriptionId);

    Task<MembersAdminPageViewModel> GetMembersViewModel(AdminServiceRequest request);

    Task<MemberAdminPageViewModel> GetMemberViewModel(AdminServiceRequest request, Guid memberId);

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
