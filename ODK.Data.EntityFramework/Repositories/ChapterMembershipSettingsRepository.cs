using ODK.Core.Chapters;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Caching;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;
public class ChapterMembershipSettingsRepository : CachingWriteRepositoryBase<ChapterMembershipSettings, Guid>, IChapterMembershipSettingsRepository
{
    private static readonly EntityCache<Guid, ChapterMembershipSettings> _cache 
        = new EntityCache<Guid, ChapterMembershipSettings>(x => x.ChapterId);

    public ChapterMembershipSettingsRepository(OdkContext context) 
        : base(context, _cache)
    {
    }

    public IDeferredQuerySingleOrDefault<ChapterMembershipSettings> GetByChapterId(Guid chapterId) => Set()
        .Where(x => x.ChapterId == chapterId)
        .DeferredSingleOrDefault(
            () => _cache.Get(chapterId),
            _cache.Set);
}
