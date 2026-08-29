using ODK.Core.Chapters;
using ODK.Core.Platforms;
using ODK.Data.Core.Chapters;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;

public interface IChapterSubscriptionRepository : IReadWriteRepository<ChapterSubscription>
{
    IDeferredQueryMultiple<ChapterSubscriptionAdminDto> GetAdminDtosByChapterId(
        Guid chapterId, EnvironmentType environment, bool includeDisabled);

    IDeferredQueryMultiple<ChapterSubscription> GetByChapterId(
        Guid chapterId, EnvironmentType environment, bool includeDisabled);

    IDeferredQuery<bool> InUse(Guid chapterSubscriptionId);
}