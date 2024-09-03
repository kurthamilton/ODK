using ODK.Services.Members.ViewModels;

namespace ODK.Services.Members;

public interface IMemberViewModelService
{
    Task<MemberPageViewModel> GetGroupMemberPage(Guid currentMemberId, string slug, Guid memberId);

    Task<MembersPageViewModel> GetGroupMembersPage(Guid currentMemberId, string slug);

    Task<MemberPageViewModel> GetMemberPage(Guid currentMemberId, string chapterName, Guid memberId);

    Task<MembersPageViewModel> GetMembersPage(Guid currentMemberId, string chapterName);    
}
