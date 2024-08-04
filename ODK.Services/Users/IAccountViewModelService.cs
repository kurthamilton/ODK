using ODK.Services.Users.ViewModels;

namespace ODK.Services.Users;

public interface IAccountViewModelService
{
    Task<AccountPageViewModel> GetAccountPage(Guid currentMemberId, string chapterName);

    Task<AccountViewModel> GetAccountViewModel(Guid currentMemberId, string chapterName);

    Task<JoinPageViewModel> GetJoinPage(string chapterName);
}
