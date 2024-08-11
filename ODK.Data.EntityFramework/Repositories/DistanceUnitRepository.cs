using ODK.Core.Countries;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Caching;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class DistanceUnitRepository : CachingReadWriteRepositoryBase<DistanceUnit>, IDistanceUnitRepository
{
    private static readonly EntityCache<Guid, DistanceUnit> _cache = new DatabaseEntityCache<DistanceUnit>();

    public DistanceUnitRepository(OdkContext context) 
        : base(context, _cache)
    {
    }

    public IDeferredQueryMultiple<DistanceUnit> GetAll() => Set()
        .DeferredMultiple(
            _cache.GetAll,
            _cache.SetAll);
}
