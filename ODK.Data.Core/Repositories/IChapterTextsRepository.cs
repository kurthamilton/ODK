using ODK.Core.Chapters;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;
public interface IChapterTextsRepository : IWriteRepository<ChapterTexts>
{
    IDeferredQuerySingleOrDefault<ChapterTexts> GetByChapterId(Guid chapterId);

    IDeferredQueryMultiple<ChapterTexts> GetByChapterIds(IEnumerable<Guid> chapterIds);
}
