using ODK.Core.Chapters;
using ODK.Data.Core.Chapters;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;

public interface IChapterLocationRepository : IWriteRepository<ChapterLocation>
{
    Task<ChapterLocation?> GetByChapterId(Guid chapterId);

    IDeferredQueryMultiple<ChapterLocationDto> GetDtosByChapterIds(IEnumerable<Guid> chapterIds);

    IDeferredQuerySingleOrDefault<ChapterLocationDto> GetDtoByChapterId(Guid chapterId);
}