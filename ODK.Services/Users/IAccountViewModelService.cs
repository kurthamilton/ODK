using ODK.Core.Chapters;
using ODK.Services.Users.ViewModels;

namespace ODK.Services.Users;

public interface IAccountViewModelService
{
    Task<AccountCreatePageViewModel> GetAccountCreatePage();

    Task<ChapterJoinPageViewModel> GetChapterJoinPage(ServiceRequest request, Chapter chapter);

    Task<ChapterLoginPageViewModel> GetChapterLoginPage();

    Task<ChapterPicturePageViewModel> GetChapterPicturePage(Guid currentMemberId, Chapter chapter);

    Task<ChapterProfilePageViewModel> GetChapterProfilePage(MemberServiceRequest request, Chapter chapter);

    Task<MemberChapterPaymentsPageViewModel> GetMemberChapterPaymentsPage(
        MemberServiceRequest request, Chapter chapter);

    Task<MemberEmailPreferencesPageViewModel> GetMemberEmailPreferencesPage(MemberServiceRequest request);

    Task<MemberPaymentsPageViewModel> GetMemberPaymentsPage(MemberServiceRequest request);

    Task<SiteLoginPageViewModel> GetSiteLoginPage();

    Task<SitePicturePageViewModel> GetSitePicturePage(Guid currentMemberId);
}