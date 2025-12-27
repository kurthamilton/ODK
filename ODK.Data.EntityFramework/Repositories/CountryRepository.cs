using ODK.Core.Chapters;
using ODK.Core.Countries;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Caching;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class CountryRepository : CachingReadWriteRepositoryBase<Country>, ICountryRepository
{
    private static readonly EntityCache<Guid, Country> _cache = new DatabaseEntityCache<Country>();

    public CountryRepository(OdkContext context)
        : base(context, _cache)
    {
    }

    public IDeferredQueryMultiple<Country> GetAll() => Set()
        .DeferredMultiple(
            _cache.GetAll,
            _cache.SetAll);

    public IDeferredQuerySingle<Country> GetByChapterId(Guid chapterId)
    {
        var query = 
            from country in Set()
            from chapter in Set<Chapter>()
                .Where(x => country.Id == x.CountryId)
            where chapter.Id == chapterId
            select country;

        return query.DeferredSingle();
    }
}
