using ODK.Core.Chapters;
using ODK.Core.Chapters.Dtos;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;

public interface IChapterSubscriptionRepository : IReadWriteRepository<ChapterSubscription>
{
    IDeferredQueryMultiple<ChapterSubscriptionAdminDto> GetAdminDtosByChapterId(
        Guid chapterId, bool includeDisabled);

    IDeferredQueryMultiple<ChapterSubscription> GetByChapterId(Guid chapterId, bool includeDisabled);

    IDeferredQuerySingle<ChapterSubscriptionDto> GetDtoById(Guid id);

    IDeferredQueryMultiple<ChapterSubscriptionDto> GetDtosByChapterId(Guid chapterId, bool includeDisabled);

    IDeferredQuery<bool> InUse(Guid chapterSubscriptionId);
}
