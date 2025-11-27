using ODK.Core.Members;
using ODK.Core.Web;
using ODK.Services.Members.Models;
using ODK.Services.Members.ViewModels;

namespace ODK.Services.Members;

public interface IMemberAdminService
{    
    Task<ServiceResult> ApproveMember(MemberChapterServiceRequest request, Guid memberId);

    Task<AdminMemberAdminPageViewModel> GetAdminMemberViewModel(MemberChapterServiceRequest request, Guid memberId);

    Task<AdminMembersAdminPageViewModel> GetAdminMembersAdminPageViewModel(MemberChapterServiceRequest request);

    Task<BulkEmailAdminPageViewModel> GetBulkEmailViewModel(MemberChapterServiceRequest request);

    Task<Member> GetMember(MemberChapterServiceRequest request, Guid memberId);

    Task<MemberApprovalsAdminPageViewModel> GetMemberApprovalsViewModel(MemberChapterServiceRequest request);

    Task<MemberAvatar?> GetMemberAvatar(MemberChapterServiceRequest request, Guid memberId);

    Task<MemberConversationsAdminPageViewModel> GetMemberConversationsViewModel(MemberChapterServiceRequest request, Guid memberId);

    Task<IReadOnlyCollection<IReadOnlyCollection<string>>> GetMemberCsv(MemberChapterServiceRequest request);

    Task<MemberDeleteAdminPageViewModel> GetMemberDeleteViewModel(MemberChapterServiceRequest request, Guid memberId);

    Task<MemberEventsAdminPageViewModel> GetMemberEventsViewModel(MemberChapterServiceRequest request, Guid memberId);

    Task<MemberImageAdminPageViewModel> GetMemberImageViewModel(MemberChapterServiceRequest request, Guid memberId);

    Task<MemberPaymentsAdminPageViewModel> GetMemberPaymentsViewModel(MemberChapterServiceRequest request, Guid memberId);

    Task<SubscriptionCreateAdminPageViewModel> GetMemberSubscriptionCreateViewModel(MemberChapterServiceRequest request);

    Task<SubscriptionsAdminPageViewModel> GetMemberSubscriptionsViewModel(MemberChapterServiceRequest request);

    Task<SubscriptionAdminPageViewModel> GetMemberSubscriptionViewModel(MemberChapterServiceRequest request, Guid subscriptionId);

    Task<MembersAdminPageViewModel> GetMembersViewModel(MemberChapterServiceRequest request);

    Task<MemberAdminPageViewModel> GetMemberViewModel(MemberChapterServiceRequest request, Guid memberId);

    Task<ServiceResult> RemoveMemberFromChapter(MemberChapterServiceRequest request, Guid memberId, string? reason);

    Task RotateMemberImage(MemberChapterServiceRequest request, Guid memberId);

    Task SendActivationEmail(MemberChapterServiceRequest request, Guid memberId);

    Task<ServiceResult> SendBulkEmail(MemberChapterServiceRequest request, MemberFilter filter, string subject, string body);

    Task SendMemberSubscriptionReminderEmails(IHttpRequestContext httpRequestContext);

    Task SetMemberVisibility(MemberChapterServiceRequest request, Guid memberId, bool visible);

    Task<ServiceResult> UpdateMemberImage(MemberChapterServiceRequest request, Guid id,
        UpdateMemberImage model);

    Task<ServiceResult> UpdateMemberSubscription(MemberChapterServiceRequest request, Guid memberId, 
        UpdateMemberSubscription subscription);
}
