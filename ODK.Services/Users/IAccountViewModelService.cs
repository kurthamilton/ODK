using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Services.Users.ViewModels;

namespace ODK.Services.Users;

public interface IAccountViewModelService
{
    Task<AccountCreatePageViewModel> GetAccountCreatePage();

    Task<ChapterJoinPageViewModel> GetChapterJoinPage(IChapterServiceRequest request);

    Task<ChapterLoginPageViewModel> GetChapterLoginPage();

    Task<ChapterPicturePageViewModel> GetChapterPicturePage(IMemberChapterServiceRequest request);

    Task<ChapterProfilePageViewModel> GetChapterProfilePage(IMemberChapterServiceRequest request);

    Task<MemberChapterPaymentsPageViewModel> GetMemberChapterPaymentsPage(
        IMemberChapterServiceRequest request);

    Task<MemberEmailPreferencesPageViewModel> GetMemberEmailPreferencesPage(IMemberServiceRequest request);

    Task<MemberPaymentsPageViewModel> GetMemberPaymentsPage(IMemberServiceRequest request);

    Task<SiteLoginPageViewModel> GetSiteLoginPage();

    Task<SitePicturePageViewModel> GetSitePicturePage(Member currentMember);
}