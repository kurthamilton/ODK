using ODK.Services.Users.ViewModels;

namespace ODK.Services.Users;

public interface IAccountViewModelService
{
    Task<AccountCreatePageViewModel> GetAccountCreatePage();

    Task<ChapterAccountViewModel> GetChapterAccountViewModel(Guid currentMemberId, string chapterName);

    Task<ChapterJoinPageViewModel> GetChapterJoinPage(string chapterName);

    Task<ChapterPicturePageViewModel> GetChapterPicturePage(Guid currentMemberId, string chapterName);

    Task<ChapterProfilePageViewModel> GetChapterProfilePage(Guid currentMemberId, string chapterName);

    Task<MemberChapterPaymentsPageViewModel> GetMemberChapterPaymentsPage(Guid currentMemberId, string chapterName);

    Task<MemberEmailPreferencesPageViewModel> GetMemberEmailPreferencesPage(Guid currentMemberId);

    Task<MemberPaymentsPageViewModel> GetMemberPaymentsPage(Guid currentMemberId);

    Task<SiteLoginPageViewModel> GetSiteLoginPage();

    Task<SitePicturePageViewModel> GetSitePicturePage(Guid currentMemberId);
}
