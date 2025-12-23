using ODK.Services.Users.ViewModels;

namespace ODK.Services.Users;

public interface IAccountViewModelService
{
    Task<AccountCreatePageViewModel> GetAccountCreatePage();

    Task<ChapterAccountViewModel> GetChapterAccountViewModel(Guid currentMemberId, string chapterName);

    Task<ChapterJoinPageViewModel> GetChapterJoinPage(ServiceRequest request, string chapterName);

    Task<ChapterLoginPageViewModel> GetChapterLoginPage();

    Task<ChapterPicturePageViewModel> GetChapterPicturePage(Guid currentMemberId, string chapterName);

    Task<ChapterProfilePageViewModel> GetChapterProfilePage(MemberServiceRequest request, string chapterName);

    Task<MemberChapterPaymentsPageViewModel> GetMemberChapterPaymentsPage(
        MemberServiceRequest request, string chapterName);

    Task<MemberEmailPreferencesPageViewModel> GetMemberEmailPreferencesPage(MemberServiceRequest request);

    Task<MemberPaymentsPageViewModel> GetMemberPaymentsPage(MemberServiceRequest request);

    Task<SiteLoginPageViewModel> GetSiteLoginPage();

    Task<SitePicturePageViewModel> GetSitePicturePage(Guid currentMemberId);
}
