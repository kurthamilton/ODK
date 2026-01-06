using ODK.Core.Chapters;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;
public interface IChapterPropertyRepository : IReadWriteRepository<ChapterProperty>
{
    IDeferredQuery<bool> ChapterHasProperties(Guid chapterId);

    IDeferredQueryMultiple<ChapterProperty> GetByChapterId(Guid chapterId);
}
