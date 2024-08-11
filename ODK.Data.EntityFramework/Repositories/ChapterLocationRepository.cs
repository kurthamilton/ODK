using Microsoft.EntityFrameworkCore;
using ODK.Core.Chapters;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Caching;

namespace ODK.Data.EntityFramework.Repositories;

public class ChapterLocationRepository : CachingWriteRepositoryBase<ChapterLocation, Guid>, IChapterLocationRepository
{
    private static readonly EntityCache<Guid, ChapterLocation> _cache = new EntityCache<Guid, ChapterLocation>(x => x.ChapterId);

    public ChapterLocationRepository(OdkContext context) 
        : base(context, _cache)
    {
    }

    public async Task<IReadOnlyCollection<ChapterLocation>> GetAll()
    {
        var cached = _cache.GetAll();
        if (cached != null)
        {
            return cached;
        }

        var chapterLocations = await Set().ToArrayAsync();
        _cache.SetAll(chapterLocations);
        return chapterLocations;
    }

    public Task<ChapterLocation?> GetByChapterId(Guid chapterId) => Set()
        .Where(x => x.ChapterId == chapterId)
        .FirstOrDefaultAsync();
}
