using ODK.Services.Chapters.ViewModels;

namespace ODK.Services.Chapters;

public interface IChapterSiteAdminService
{
    Task<ServiceResult> ApproveChapter(MemberServiceRequest request, Guid chapterId);

    Task<ServiceResult> DeleteChapter(MemberServiceRequest request, Guid chapterId);

    Task<ChapterPaymentSettingsAdminPageViewModel> GetChapterPaymentSettingsViewModel(
        MemberChapterServiceRequest request);

    Task<SiteAdminChaptersViewModel> GetSiteAdminChaptersViewModel(MemberServiceRequest request);

    Task<SiteAdminChapterViewModel> GetSiteAdminChapterViewModel(MemberChapterServiceRequest request);

    Task<ServiceResult> UpdateSiteAdminChapter(
        MemberChapterServiceRequest request,
        SiteAdminChapterUpdateViewModel viewModel);
}
