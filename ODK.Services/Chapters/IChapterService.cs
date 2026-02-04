using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Core.Platforms;
using ODK.Services.Chapters.ViewModels;
using ODK.Services.Members.ViewModels;

namespace ODK.Services.Chapters;

public interface IChapterService
{
    Task<Chapter> GetByEventId(ServiceRequest request, Guid eventId);

    Task<VersionedServiceResult<ChapterImage>> GetChapterImage(long? currentVersion, Guid chapterId);

    Task<ChapterLayoutViewModel> GetChapterLayoutViewModel(Guid chapterId);

    Task<SubscriptionsPageViewModel> GetChapterMemberSubscriptionsViewModel(MemberChapterServiceRequest request);

    Task<IReadOnlyCollection<Chapter>> GetChaptersByOwnerId(ServiceRequest request, Guid ownerId);

    Task<ChaptersHomePageViewModel> GetChaptersHomePageViewModel(PlatformType platform);

    Task<Chapter?> GetDefaultChapter(MemberServiceRequest request);

    Task<bool> NameIsAvailable(ServiceRequest request, string name);
}