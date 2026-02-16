using Microsoft.EntityFrameworkCore;
using ODK.Core.Chapters;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class ChapterLocationRepository : WriteRepositoryBase<ChapterLocation>, IChapterLocationRepository
{
    public ChapterLocationRepository(OdkContext context)
        : base(context)
    {
    }

    public IDeferredQuerySingleOrDefault<ChapterLocation> GetByChapterId(Guid chapterId)
        => Set()
            .Where(x => x.ChapterId == chapterId)
            .DeferredSingleOrDefault();

    public IDeferredQueryMultiple<ChapterLocation> GetByChapterIds(IEnumerable<Guid> chapterIds)
        => Set()
            .Where(x => chapterIds.Contains(x.ChapterId))
            .DeferredMultiple();
}