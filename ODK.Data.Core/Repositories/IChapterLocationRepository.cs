using ODK.Core.Chapters;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;

public interface IChapterLocationRepository : IWriteRepository<ChapterLocation>
{
    IDeferredQueryMultiple<ChapterLocation> GetByChapterIds(IEnumerable<Guid> chapterIds);

    IDeferredQuerySingleOrDefault<ChapterLocation> GetByChapterId(Guid chapterId);
}