using ODK.Core.Countries;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Caching;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class CurrencyRepository : ReadWriteRepositoryBase<Currency>, ICurrencyRepository
{
    public CurrencyRepository(OdkContext context) 
        : base(context)
    {
    }

    public IDeferredQueryMultiple<Currency> GetAll() => Set()
        .DeferredMultiple();

    public IDeferredQuerySingleOrDefault<Currency> GetByCode(string code)
        => Set()
            .Where(x => x.Code == code)
            .DeferredSingleOrDefault();
}
