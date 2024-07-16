using ODK.Core.Chapters;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;
public interface IChapterLinksRepository : IWriteRepository<ChapterLinks>
{
    IDeferredQuerySingleOrDefault<ChapterLinks> GetByChapterId(Guid chapterId);
}
