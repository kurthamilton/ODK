using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Core.Platforms;
using ODK.Services.Chapters.ViewModels;
using ODK.Services.Members.ViewModels;

namespace ODK.Services.Chapters;

public interface IChapterService
{
    Task<VersionedServiceResult<ChapterImage>> GetChapterImage(long? currentVersion, Guid chapterId);

    Task<ChapterLayoutViewModel> GetChapterLayoutViewModel(Guid chapterId);

    Task<SubscriptionsPageViewModel> GetChapterMemberSubscriptionsViewModel(MemberChapterServiceRequest request);

    Task<IReadOnlyCollection<Chapter>> GetChaptersByOwnerId(Guid ownerId);

    Task<ChaptersHomePageViewModel> GetChaptersHomePageViewModel(PlatformType platform);

    Task<Chapter?> GetDefaultChapter(Member member);

    Task<IReadOnlyCollection<Chapter>> GetMemberChapters(Guid memberId);

    Task<bool> NameIsAvailable(string name);
}