using ODK.Core.Chapters;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;

public interface IChapterEventSettingsRepository : IWriteRepository<ChapterEventSettings>
{
    IDeferredQuerySingleOrDefault<ChapterEventSettings> GetByChapterId(Guid chapterId);
}
