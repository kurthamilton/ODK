using ODK.Core.Members;
using ODK.Services.Members.Models;
using ODK.Services.Members.ViewModels;

namespace ODK.Services.Members;

public interface IMemberAdminService
{
    Task<ServiceResult> ApproveMember(IMemberChapterAdminServiceRequest request, Guid memberId);

    Task<AdminMemberAdminPageViewModel> GetAdminMemberViewModel(IMemberChapterAdminServiceRequest request, Guid memberId);

    Task<AdminMembersAdminPageViewModel> GetAdminMembersAdminPageViewModel(IMemberChapterAdminServiceRequest request);

    /// <summary>
    /// Everyone the group has asked to join who has yet to accept, oldest invitation first.
    /// </summary>
    Task<InvitedMembersAdminPageViewModel> GetInvitedMembersViewModel(
        IMemberChapterAdminServiceRequest request);

    Task<Member> GetMember(IMemberChapterAdminServiceRequest request, Guid memberId);

    Task<MemberApprovalsAdminPageViewModel> GetMemberApprovalsViewModel(
        IMemberChapterAdminServiceRequest request);

    Task<MemberConversationsAdminPageViewModel> GetMemberConversationsViewModel(
        IMemberChapterAdminServiceRequest request, Guid memberId);

    Task<IReadOnlyCollection<IReadOnlyCollection<string>>> GetMemberCsv(
        IMemberChapterAdminServiceRequest request);

    Task<MemberImportPreview> GetMemberImportPreview(
        IMemberChapterAdminServiceRequest request, IReadOnlyCollection<MemberImportModel> members);

    Task<IReadOnlyCollection<IReadOnlyCollection<string>>> GetMemberImportTemplate(
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

    Task<ServiceResult> ImportMembers(IMemberChapterAdminServiceRequest request, IReadOnlyCollection<MemberImportModel> members);

    Task<ServiceResult> RemoveMemberFromChapter(IMemberChapterAdminServiceRequest request, Guid memberId, string? reason);

    Task RotateMemberImage(IMemberChapterAdminServiceRequest request, Guid memberId);

    Task SendActivationEmail(IMemberChapterAdminServiceRequest request, Guid memberId);

    /// <summary>
    /// Sends one email to the selected members. An id that names nobody the group can email - someone
    /// outside it, an account not yet activated, a member who has turned the group's emails off - is
    /// dropped rather than refused, and the result says how many were actually written to. Only a selection
    /// that reaches nobody at all fails.
    /// </summary>
    Task<ServiceResult> SendBulkEmail(
        IMemberChapterAdminServiceRequest request, IReadOnlyCollection<Guid> memberIds, string subject, string body);

    Task SendMemberSubscriptionReminderEmails(IServiceRequest request);

    Task SetMemberVisibility(IMemberChapterServiceRequest request, Guid memberId, bool visible);

    Task<ServiceResult> UpdateMemberImage(
        IMemberChapterAdminServiceRequest request,
        Guid id,
        MemberImageUpdateModel model);

    /// <summary>
    /// Site-admin only: members whose signup scored below the reCAPTCHA threshold, newest first.
    /// </summary>
    Task<SiteAdminFlaggedMembersViewModel> GetSiteAdminFlaggedMembersViewModel(IMemberServiceRequest request);

    /// <summary>
    /// Site-admin only: members matching <paramref name="search"/>, for picking one to sign in as. A blank
    /// search matches nobody rather than everybody - the page is a lookup, not a member list.
    /// <paramref name="signedInMemberIds"/> is what the caller's auth cookie already holds, which decides
    /// whether a row offers to sign that member in or out.
    /// </summary>
    Task<SiteAdminMemberSearchViewModel> GetSiteAdminMemberSearchViewModel(
        IMemberServiceRequest request,
        string? search,
        IReadOnlyCollection<Guid> signedInMemberIds);

    Task<ServiceResult> UpdateMemberSubscription(
        IMemberChapterAdminServiceRequest request,
        Guid memberId,
        MemberSubscriptionUpdateModel subscription);
}