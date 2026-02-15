using Microsoft.EntityFrameworkCore;
using ODK.Core.Chapters;
using ODK.Data.Core.Chapters;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;
using ODK.Data.EntityFramework.Queries;

namespace ODK.Data.EntityFramework.Repositories;

public class ChapterLocationRepository : WriteRepositoryBase<ChapterLocation>, IChapterLocationRepository
{
    public ChapterLocationRepository(OdkContext context)
        : base(context)
    {
    }

    public async Task<ChapterLocation?> GetByChapterId(Guid chapterId)
        => await Set()
            .Where(x => x.ChapterId == chapterId)
            .FirstOrDefaultAsync();

    public IDeferredQuerySingleOrDefault<ChapterLocationDto> GetDtoByChapterId(Guid chapterId)
        => Set()
            .Where(x => x.ChapterId == chapterId)
            .ToDtos()
            .DeferredSingleOrDefault();

    public IDeferredQueryMultiple<ChapterLocationDto> GetDtosByChapterIds(IEnumerable<Guid> chapterIds)
        => Set()
            .Where(x => chapterIds.Contains(x.ChapterId))
            .ToDtos()
            .DeferredMultiple();
}