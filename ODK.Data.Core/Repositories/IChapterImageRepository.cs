using ODK.Core.Chapters;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;

public interface IChapterImageRepository : IWriteRepository<ChapterImage>, IChapterEntityRepository<ChapterImage>
{
    IDeferredQuery<bool> ExistsForChapterId(Guid chapterId);

    IDeferredQuerySingleOrDefault<ChapterImage> GetByChapterId(Guid chapterId);

    IDeferredQueryMultiple<ChapterImageMetadata> GetDtosByChapterIds(IEnumerable<Guid> chapterIds);
}
