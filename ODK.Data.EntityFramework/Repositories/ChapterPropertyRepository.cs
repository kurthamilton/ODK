using Microsoft.EntityFrameworkCore;
using ODK.Core.Chapters;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class ChapterPropertyRepository : ReadWriteRepositoryBase<ChapterProperty>, IChapterPropertyRepository
{
    public ChapterPropertyRepository(DbContext context)
        : base(context)
    {
    }

    public IDeferredQuery<bool> ChapterHasProperties(Guid chapterId)
        => Set()
            .Where(x => x.ChapterId == chapterId)
            .DeferredAny();

    public IDeferredQueryMultiple<ChapterProperty> GetByChapterId(Guid chapterId)
        => Set()
            .Where(x => x.ChapterId == chapterId)
            .DeferredMultiple();

    public IDeferredQueryMultiple<ChapterProperty> GetByChapterIds(IEnumerable<Guid> chapterIds)
        => Set()
            .Where(x => chapterIds.Contains(x.ChapterId))
            .DeferredMultiple();
}