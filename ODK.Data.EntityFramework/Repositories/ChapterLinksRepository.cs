using ODK.Core.Chapters;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Caching;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;
public class ChapterLinksRepository : CachingWriteRepositoryBase<ChapterLinks, Guid>, IChapterLinksRepository
{
    private static readonly EntityCache<Guid, ChapterLinks> _cache = new EntityCache<Guid, ChapterLinks>(x => x.ChapterId);

    public ChapterLinksRepository(OdkContext context) 
        : base(context, _cache)
    {
    }

    public IDeferredQuerySingleOrDefault<ChapterLinks> GetByChapterId(Guid chapterId) => Set()
        .Where(x => x.ChapterId == chapterId)
        .DeferredSingleOrDefault(
            () => _cache.Get(chapterId),
            _cache.Set);
}
