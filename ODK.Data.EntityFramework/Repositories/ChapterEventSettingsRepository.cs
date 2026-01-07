using ODK.Core.Chapters;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Caching;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class ChapterEventSettingsRepository : CachingWriteRepositoryBase<ChapterEventSettings, Guid>, IChapterEventSettingsRepository
{
    private static readonly EntityCache<Guid, ChapterEventSettings> _cache = new EntityCache<Guid, ChapterEventSettings>(x => x.ChapterId);

    public ChapterEventSettingsRepository(OdkContext context)
        : base(context, _cache)
    {
    }

    public IDeferredQuerySingleOrDefault<ChapterEventSettings> GetByChapterId(Guid chapterId) => Set()
        .Where(x => x.ChapterId == chapterId)
        .DeferredSingleOrDefault(
            () => _cache.Get(chapterId),
            _cache.Set);
}
