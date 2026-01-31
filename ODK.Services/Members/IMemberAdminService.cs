using ODK.Core.Members;
using ODK.Services.Members.Models;
using ODK.Services.Members.ViewModels;

namespace ODK.Services.Members;

public interface IMemberAdminService
{
    Task<ServiceResult> ApproveMember(MemberChapterAdminServiceRequest request, Guid memberId);

    Task<AdminMemberAdminPageViewModel> GetAdminMemberViewModel(MemberChapterAdminServiceRequest request, Guid memberId);

    Task<AdminMembersAdminPageViewModel> GetAdminMembersAdminPageViewModel(MemberChapterAdminServiceRequest request);

    Task<BulkEmailAdminPageViewModel> GetBulkEmailViewModel(MemberChapterAdminServiceRequest request);

    Task<Member> GetMember(MemberChapterAdminServiceRequest request, Guid memberId);

    Task<MemberApprovalsAdminPageViewModel> GetMemberApprovalsViewModel(
        MemberChapterAdminServiceRequest request);

    Task<MemberAvatar?> GetMemberAvatar(MemberChapterAdminServiceRequest request, Guid memberId);

    Task<MemberConversationsAdminPageViewModel> GetMemberConversationsViewModel(
        MemberChapterAdminServiceRequest request, Guid memberId);

    Task<IReadOnlyCollection<IReadOnlyCollection<string>>> GetMemberCsv(
        MemberChapterAdminServiceRequest request);

    Task<MemberDeleteAdminPageViewModel> GetMemberDeleteViewModel(
        MemberChapterAdminServiceRequest request, Guid memberId);

    Task<MemberEventsAdminPageViewModel> GetMemberEventsViewModel(
        MemberChapterAdminServiceRequest request, Guid memberId);

    Task<MemberImageAdminPageViewModel> GetMemberImageViewModel(
        MemberChapterAdminServiceRequest request, Guid memberId);

    Task<MemberPaymentsAdminPageViewModel> GetMemberPaymentsViewModel(
        MemberChapterAdminServiceRequest request, Guid memberId);

    Task<SubscriptionCreateAdminPageViewModel> GetMemberSubscriptionCreateViewModel(
        MemberChapterAdminServiceRequest request);

    Task<SubscriptionsAdminPageViewModel> GetMemberSubscriptionsViewModel(
        MemberChapterAdminServiceRequest request);

    Task<SubscriptionAdminPageViewModel> GetMemberSubscriptionViewModel(
        MemberChapterAdminServiceRequest request, Guid subscriptionId);

    Task<MembersAdminPageViewModel> GetMembersViewModel(MemberChapterAdminServiceRequest request);

    Task<MemberAdminPageViewModel> GetMemberViewModel(MemberChapterAdminServiceRequest request, Guid memberId);

    Task<ServiceResult> RemoveMemberFromChapter(MemberChapterAdminServiceRequest request, Guid memberId, string? reason);

    Task RotateMemberImage(MemberChapterAdminServiceRequest request, Guid memberId);

    Task SendActivationEmail(MemberChapterAdminServiceRequest request, Guid memberId);

    Task<ServiceResult> SendBulkEmail(
        MemberChapterAdminServiceRequest request, MemberFilter filter, string subject, string body);

    Task SendMemberSubscriptionReminderEmails(ServiceRequest request);

    Task SetMemberVisibility(MemberChapterServiceRequest request, Guid memberId, bool visible);

    Task<ServiceResult> UpdateMemberImage(
        MemberChapterAdminServiceRequest request, 
        Guid id,
        UpdateMemberImage model);

    Task<ServiceResult> UpdateMemberSubscription(
        MemberChapterAdminServiceRequest request, 
        Guid memberId,
        UpdateMemberSubscription subscription);
}
