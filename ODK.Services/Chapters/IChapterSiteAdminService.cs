using ODK.Services.Chapters.ViewModels;

namespace ODK.Services.Chapters;

public interface IChapterSiteAdminService
{
    Task<ServiceResult> ApproveChapter(MemberServiceRequest request, Guid chapterId);

    Task<ServiceResult> DeleteChapter(MemberServiceRequest request, Guid chapterId);

    Task<ChapterPaymentSettingsAdminPageViewModel> GetChapterPaymentSettingsViewModel(
        MemberServiceRequest request, Guid chapterId);

    Task<SiteAdminChaptersViewModel> GetSiteAdminChaptersViewModel(MemberServiceRequest request);

    Task<SiteAdminChapterViewModel> GetSiteAdminChapterViewModel(MemberServiceRequest request, Guid chapterId);

    Task<ServiceResult> UpdateSiteAdminChapter(
        MemberServiceRequest request,
        Guid chapterId,
        SiteAdminChapterUpdateViewModel viewModel);
}
