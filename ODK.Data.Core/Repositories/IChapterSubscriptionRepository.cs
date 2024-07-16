using ODK.Core.Chapters;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;
public interface IChapterSubscriptionRepository : IReadWriteRepository<ChapterSubscription>
{
    IDeferredQueryMultiple<ChapterSubscription> GetByChapterId(Guid chapterId);
}
