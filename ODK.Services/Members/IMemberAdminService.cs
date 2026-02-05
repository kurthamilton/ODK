using ODK.Core.Members;
using ODK.Services.Members.Models;
using ODK.Services.Members.ViewModels;

namespace ODK.Services.Members;

public interface IMemberAdminService
{
    Task<ServiceResult> ApproveMember(IMemberChapterAdminServiceRequest request, Guid memberId);

    Task<AdminMemberAdminPageViewModel> GetAdminMemberViewModel(IMemberChapterAdminServiceRequest request, Guid memberId);

    Task<AdminMembersAdminPageViewModel> GetAdminMembersAdminPageViewModel(IMemberChapterAdminServiceRequest request);

    Task<BulkEmailAdminPageViewModel> GetBulkEmailViewModel(IMemberChapterAdminServiceRequest request);

    Task<Member> GetMember(IMemberChapterAdminServiceRequest request, Guid memberId);

    Task<MemberApprovalsAdminPageViewModel> GetMemberApprovalsViewModel(
        IMemberChapterAdminServiceRequest request);

    Task<MemberAvatar?> GetMemberAvatar(IMemberChapterAdminServiceRequest request, Guid memberId);

    Task<MemberConversationsAdminPageViewModel> GetMemberConversationsViewModel(
        IMemberChapterAdminServiceRequest request, Guid memberId);

    Task<IReadOnlyCollection<IReadOnlyCollection<string>>> GetMemberCsv(
        IMemberChapterAdminServiceRequest request);

    Task<MemberDeleteAdminPageViewModel> GetMemberDeleteViewModel(
        IMemberChapterAdminServiceRequest request, Guid memberId);

    Task<MemberEventsAdminPageViewModel> GetMemberEventsViewModel(
        IMemberChapterAdminServiceRequest request, Guid memberId);

    Task<MemberImageAdminPageViewModel> GetMemberImageViewModel(
        IMemberChapterAdminServiceRequest request, Guid memberId);

    Task<MemberPaymentsAdminPageViewModel> GetMemberPaymentsViewModel(
        IMemberChapterAdminServiceRequest request, Guid memberId);

    Task<SubscriptionCreateAdminPageViewModel> GetMemberSubscriptionCreateViewModel(
        IMemberChapterAdminServiceRequest request);

    Task<SubscriptionsAdminPageViewModel> GetMemberSubscriptionsViewModel(
        IMemberChapterAdminServiceRequest request);

    Task<SubscriptionAdminPageViewModel> GetMemberSubscriptionViewModel(
        IMemberChapterAdminServiceRequest request, Guid subscriptionId);

    Task<MembersAdminPageViewModel> GetMembersViewModel(IMemberChapterAdminServiceRequest request);

    Task<MemberAdminPageViewModel> GetMemberViewModel(IMemberChapterAdminServiceRequest request, Guid memberId);

    Task<ServiceResult> RemoveMemberFromChapter(IMemberChapterAdminServiceRequest request, Guid memberId, string? reason);

    Task RotateMemberImage(IMemberChapterAdminServiceRequest request, Guid memberId);

    Task SendActivationEmail(IMemberChapterAdminServiceRequest request, Guid memberId);

    Task<ServiceResult> SendBulkEmail(
        IMemberChapterAdminServiceRequest request, MemberFilter filter, string subject, string body);

    Task SendMemberSubscriptionReminderEmails(IServiceRequest request);

    Task SetMemberVisibility(IMemberChapterServiceRequest request, Guid memberId, bool visible);

    Task<ServiceResult> UpdateMemberImage(
        IMemberChapterAdminServiceRequest request,
        Guid id,
        MemberImageUpdateModel model);

    Task<ServiceResult> UpdateMemberSubscription(
        IMemberChapterAdminServiceRequest request,
        Guid memberId,
        MemberSubscriptionUpdateModel subscription);
}