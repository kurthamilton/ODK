using ODK.Core.Chapters;
using ODK.Services.Members.ViewModels;

namespace ODK.Services.Members;

public interface IMemberViewModelService
{
    Task<MemberPageViewModel> GetGroupMemberPage(MemberServiceRequest request, string slug, Guid memberId);

    Task<MembersPageViewModel> GetGroupMembersPage(MemberServiceRequest request, string slug);

    Task<MemberConversationsPageViewModel> GetMemberConversationsPage(MemberServiceRequest request);

    Task<MemberConversationsPageViewModel> GetMemberConversationsPage(MemberChapterServiceRequest request);

    Task<MemberInterestsPageViewModel> GetMemberInterestsPage(Guid currentMemberId);

    Task<MemberPageViewModel> GetMemberPage(MemberServiceRequest request, Chapter chapter, Guid memberId);

    Task<MembersPageViewModel> GetMembersPage(MemberServiceRequest request, Chapter chapter);
}