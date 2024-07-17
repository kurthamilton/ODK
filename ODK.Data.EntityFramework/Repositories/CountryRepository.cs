using ODK.Core.Chapters;
using ODK.Core.Countries;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class CountryRepository : ReadWriteRepositoryBase<Country>, ICountryRepository
{
    public CountryRepository(OdkContext context)
        : base(context)
    {
    }

    public IDeferredQuerySingle<Country> GetByChapterId(Guid chapterId)
    {
        var query =
            from chapter in Set<Chapter>()
            from country in Set()
            where country.Id == chapter.CountryId
                && chapter.Id == chapterId
            select country;
        return query.DeferredSingle();
    }

    public IDeferredQueryMultiple<Country> GetAll() => Set().DeferredMultiple();
}
