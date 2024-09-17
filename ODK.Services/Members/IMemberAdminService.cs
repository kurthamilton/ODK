using ODK.Core.Members;
using ODK.Services.Members.ViewModels;

namespace ODK.Services.Members;

public interface IMemberAdminService
{    
    Task<AdminMemberAdminPageViewModel> GetAdminMemberViewModel(AdminServiceRequest request, Guid memberId);

    Task<AdminMembersAdminPageViewModel> GetAdminMembersAdminPageViewModel(AdminServiceRequest request);

    Task<BulkEmailAdminPageViewModel> GetBulkEmailViewModel(AdminServiceRequest request);

    Task<Member> GetMember(AdminServiceRequest request, Guid memberId);

    Task<MemberAvatar?> GetMemberAvatar(AdminServiceRequest request, Guid memberId);

    Task<MemberConversationsAdminPageViewModel> GetMemberConversationsViewModel(AdminServiceRequest request, Guid memberId);

    Task<IReadOnlyCollection<IReadOnlyCollection<string>>> GetMemberCsv(AdminServiceRequest request);

    Task<MemberDeleteAdminPageViewModel> GetMemberDeleteViewModel(AdminServiceRequest request, Guid memberId);

    Task<MemberEventsAdminPageViewModel> GetMemberEventsViewModel(AdminServiceRequest request, Guid memberId);

    Task<MemberImageAdminPageViewModel> GetMemberImageViewModel(AdminServiceRequest request, Guid memberId);

    Task<SubscriptionCreateAdminPageViewModel> GetMemberSubscriptionCreateViewModel(AdminServiceRequest request);

    Task<SubscriptionsAdminPageViewModel> GetMemberSubscriptionsViewModel(AdminServiceRequest request);

    Task<SubscriptionAdminPageViewModel> GetMemberSubscriptionViewModel(AdminServiceRequest request, Guid subscriptionId);

    Task<MembersAdminPageViewModel> GetMembersViewModel(AdminServiceRequest request);

    Task<MemberAdminPageViewModel> GetMemberViewModel(AdminServiceRequest request, Guid memberId);

    Task<ServiceResult> RemoveMemberFromChapter(AdminServiceRequest request, Guid memberId, string? reason);

    Task RotateMemberImage(AdminServiceRequest request, Guid memberId);

    Task SendActivationEmail(AdminServiceRequest request, Guid memberId);

    Task<ServiceResult> SendBulkEmail(AdminServiceRequest request, MemberFilter filter, string subject, string body);

    Task SendMemberSubscriptionReminderEmails();

    Task SetMemberVisibility(AdminServiceRequest request, Guid memberId, bool visible);

    Task<ServiceResult> UpdateMemberImage(AdminServiceRequest request, Guid id,
        UpdateMemberImage model);

    Task<ServiceResult> UpdateMemberSubscription(AdminServiceRequest request, Guid memberId, 
        UpdateMemberSubscription subscription);
}
