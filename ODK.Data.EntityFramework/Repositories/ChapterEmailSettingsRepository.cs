using ODK.Core.Chapters;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Caching;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class ChapterEmailSettingsRepository : CachingWriteRepositoryBase<ChapterEmailSettings, Guid>, IChapterEmailSettingsRepository
{
    private static readonly EntityCache<Guid, ChapterEmailSettings> _cache = new EntityCache<Guid, ChapterEmailSettings>(x => x.ChapterId);

    public ChapterEmailSettingsRepository(OdkContext context) 
        : base(context, _cache)
    {
    }

    public IDeferredQuerySingleOrDefault<ChapterEmailSettings> GetByChapterId(Guid chapterId) => Set()
        .Where(x => x.ChapterId == chapterId)
        .DeferredSingleOrDefault(
            () => _cache.Get(chapterId),
            _cache.Set);
}
