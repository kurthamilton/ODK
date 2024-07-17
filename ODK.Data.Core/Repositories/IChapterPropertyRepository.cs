using ODK.Core.Chapters;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;
public interface IChapterPropertyRepository : IReadWriteRepository<ChapterProperty>
{
    IDeferredQueryMultiple<ChapterProperty> GetByChapterId(Guid chapterId, bool all = false);
}
