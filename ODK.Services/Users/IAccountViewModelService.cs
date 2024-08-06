using ODK.Services.Users.ViewModels;

namespace ODK.Services.Users;

public interface IAccountViewModelService
{
    Task<ChapterAccountViewModel> GetChapterAccountViewModel(Guid currentMemberId, string chapterName);

    Task<ChapterJoinPageViewModel> GetChapterJoinPage(string chapterName);

    Task<ChapterPicturePageViewModel> GetChapterPicturePage(Guid currentMemberId, string chapterName);

    Task<ChapterProfilePageViewModel> GetChapterProfilePage(Guid currentMemberId, string chapterName);

    Task<SitePicturePageViewModel> GetSitePicturePage(Guid currentMemberId);        
}
