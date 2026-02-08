using ODK.Core.Chapters;
using ODK.Core.Platforms;
using ODK.Services.Chapters.ViewModels;
using ODK.Services.Members.ViewModels;

namespace ODK.Services.Chapters;

public interface IChapterService
{
    Task<Chapter> GetByEventId(IServiceRequest request, Guid eventId);

    Task<VersionedServiceResult<ChapterImage>> GetChapterImage(long? currentVersion, Guid chapterId);

    Task<ChapterLayoutViewModel> GetChapterLayoutViewModel(Guid chapterId);

    Task<SubscriptionsPageViewModel> GetChapterMemberSubscriptionsViewModel(IMemberChapterServiceRequest request);

    Task<IReadOnlyCollection<Chapter>> GetChaptersByOwnerId(IServiceRequest request, Guid ownerId);

    Task<ChaptersHomePageViewModel> GetChaptersHomePageViewModel(PlatformType platform);

    Task<Chapter?> GetDefaultChapter(IMemberServiceRequest request);
}