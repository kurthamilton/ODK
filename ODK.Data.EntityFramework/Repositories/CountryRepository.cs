using ODK.Core.Countries;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Caching;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class CountryRepository : CachingReadWriteRepositoryBase<Country>, ICountryRepository
{
    private static readonly EntityCache<Guid, Country> _cache = new EntityCache<Guid, Country>(x => x.Id);

    public CountryRepository(OdkContext context)
        : base(context, _cache)
    {
    }

    public IDeferredQueryMultiple<Country> GetAll() => Set()
        .DeferredMultiple(
            _cache.GetAll,
            _cache.SetAll);
}
