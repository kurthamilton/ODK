using ODK.Services.Members.ViewModels;

namespace ODK.Services.Members;

public interface IMemberViewModelService
{
    Task<MemberPageViewModel> GetMemberPage(Guid currentMemberId, string chapterName, Guid memberId);

    Task<MembersPageViewModel> GetMembersPage(Guid currentMemberId, string chapterName);    
}
