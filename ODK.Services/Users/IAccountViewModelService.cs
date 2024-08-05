using ODK.Services.Users.ViewModels;

namespace ODK.Services.Users;

public interface IAccountViewModelService
{
    Task<ChapterAccountViewModel> GetChapterAccountViewModel(Guid currentMemberId, string chapterName);

    Task<ChapterAccountPageViewModel> GetChapterAccountPage(Guid currentMemberId, string chapterName);

    Task<ChapterJoinPageViewModel> GetChapterJoinPage(string chapterName);

    Task<SiteAccountPageViewModel> GetSiteAccountPage(Guid currentMemberId);        
}
