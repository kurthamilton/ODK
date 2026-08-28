using ODK.Services.Chapters.ViewModels;

namespace ODK.Services.Chapters;

public interface IChapterSiteAdminService
{
    Task<ServiceResult> ApproveChapter(IMemberServiceRequest request, Guid chapterId);

    Task<ServiceResult> DeleteChapter(IMemberServiceRequest request, Guid chapterId);

    Task<ChapterAdminMembersSiteAdminPageViewModel> GetChapterAdminMembersViewModel(
        IMemberChapterServiceRequest request);

    Task<ChapterPaymentSettingsAdminPageViewModel> GetChapterPaymentSettingsViewModel(
        IMemberChapterServiceRequest request);

    /// <summary>
    /// Every one of the group's subscriptions, including the ones its own admins cannot see, each with the
    /// payment settings it transacts through.
    /// </summary>
    Task<ChapterSubscriptionsAdminPageViewModel> GetChapterSubscriptionsViewModel(
        IMemberChapterServiceRequest request);

    Task<SiteAdminChaptersViewModel> GetSiteAdminChaptersViewModel(IMemberServiceRequest request);

    Task<SiteAdminChapterViewModel> GetSiteAdminChapterViewModel(IMemberChapterServiceRequest request);

    Task<ServiceResult> UpdateSiteAdminChapter(
        IMemberChapterServiceRequest request,
        SiteAdminChapterUpdateViewModel viewModel);
}