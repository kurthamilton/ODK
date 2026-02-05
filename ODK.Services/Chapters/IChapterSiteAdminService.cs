using ODK.Services.Chapters.ViewModels;

namespace ODK.Services.Chapters;

public interface IChapterSiteAdminService
{
    Task<ServiceResult> ApproveChapter(IMemberServiceRequest request, Guid chapterId);

    Task<ServiceResult> DeleteChapter(IMemberServiceRequest request, Guid chapterId);

    Task<ChapterPaymentSettingsAdminPageViewModel> GetChapterPaymentSettingsViewModel(
        IMemberChapterServiceRequest request);

    Task<SiteAdminChaptersViewModel> GetSiteAdminChaptersViewModel(IMemberServiceRequest request);

    Task<SiteAdminChapterViewModel> GetSiteAdminChapterViewModel(IMemberChapterServiceRequest request);

    Task<ServiceResult> UpdateSiteAdminChapter(
        IMemberChapterServiceRequest request,
        SiteAdminChapterUpdateViewModel viewModel);
}