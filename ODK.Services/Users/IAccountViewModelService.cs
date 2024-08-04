using ODK.Services.Users.ViewModels;

namespace ODK.Services.Users;

public interface IAccountViewModelService
{
    Task<AccountPageViewModel> GetAccountPageViewModel(Guid currentMemberId, string chapterName);

    Task<AccountViewModel> GetAccountViewModel(Guid currentMemberId, string chapterName);

    Task<JoinPageViewModel> GetJoinPageViewModel(string chapterName);
}
