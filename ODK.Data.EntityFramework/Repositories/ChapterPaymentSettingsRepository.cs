using Microsoft.EntityFrameworkCore;
using ODK.Core.Chapters;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Caching;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;
public class ChapterPaymentSettingsRepository : CachingWriteRepositoryBase<ChapterPaymentSettings, Guid>, IChapterPaymentSettingsRepository
{
    private static readonly EntityCache<Guid, ChapterPaymentSettings> _cache = new EntityCache<Guid, ChapterPaymentSettings>(x => x.ChapterId);

    public ChapterPaymentSettingsRepository(OdkContext context) 
        : base(context, _cache)
    {
    }

    public IDeferredQuerySingleOrDefault<ChapterPaymentSettings> GetByChapterId(Guid chapterId) => Set()
        .Where(x => x.ChapterId == chapterId)
        .DeferredSingleOrDefault(
            () => _cache.Get(chapterId),
            _cache.Set);

    protected override IQueryable<ChapterPaymentSettings> Set() => base.Set()
        .Include(x => x.Currency);
}
