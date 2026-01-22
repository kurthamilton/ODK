using ODK.Core.Chapters;
using ODK.Data.Core.Chapters;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;

public interface IChapterSubscriptionRepository : IReadWriteRepository<ChapterSubscription>
{
    IDeferredQueryMultiple<ChapterSubscriptionAdminDto> GetAdminDtosByChapterId(
        Guid chapterId, bool includeDisabled);

    IDeferredQueryMultiple<ChapterSubscription> GetByChapterId(Guid chapterId, bool includeDisabled);

    IDeferredQuery<bool> InUse(Guid chapterSubscriptionId);
}