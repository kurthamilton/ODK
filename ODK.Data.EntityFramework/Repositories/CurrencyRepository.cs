using ODK.Core.Countries;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Caching;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class CurrencyRepository : CachingReadWriteRepositoryBase<Currency>, ICurrencyRepository
{
    private static readonly EntityCache<Guid, Currency> _cache = new DatabaseEntityCache<Currency>();

    public CurrencyRepository(OdkContext context) 
        : base(context, _cache)
    {
    }

    public IDeferredQueryMultiple<Currency> GetAll() => Set()
        .DeferredMultiple(
            _cache.GetAll,
            _cache.SetAll);
}
