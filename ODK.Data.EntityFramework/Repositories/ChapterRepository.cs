using ODK.Core.Chapters;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Caching;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class ChapterRepository : CachingReadWriteRepositoryBase<Chapter>, IChapterRepository, IDisposable
{
    private static readonly EntityCache<Guid, Chapter> _cache = new EntityCache<Guid, Chapter>(x => x.Id);

    public ChapterRepository(OdkContext context)
        : base(context, _cache)
    {
    }

    public IDeferredQueryMultiple<Chapter> GetAll() => Set()
        .OrderBy(x => x.DisplayOrder)
        .DeferredMultiple(           
            _cache.GetAll,
            _cache.SetAll);    

    public IDeferredQuerySingleOrDefault<Chapter> GetByName(string name) => Set()
        .Where(x => x.Name == name)
        .DeferredSingleOrDefault(
            () => _cache.Find(x => string.Equals(x.Name, name, StringComparison.InvariantCultureIgnoreCase)),
            _cache.Set);
}
