using ODK.Core.Chapters;
using ODK.Core.Pages;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;

public interface IChapterPageRepository : IReadWriteRepository<ChapterPage>
{
    IDeferredQueryMultiple<ChapterPage> GetByChapterId(Guid chapterId);

    IDeferredQuerySingleOrDefault<ChapterPage> GetByChapterId(Guid chapterId, PageType pageType);
}