using ODK.Core.Chapters;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;
public interface IChapterPropertyOptionRepository : IReadWriteRepository<ChapterPropertyOption>
{
    IDeferredQueryMultiple<ChapterPropertyOption> GetByChapterId(Guid chapterId);
}
