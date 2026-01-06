using ODK.Core.Countries;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;
public interface ICurrencyRepository : IReadWriteRepository<Currency>
{
    IDeferredQueryMultiple<Currency> GetAll();

    IDeferredQuerySingleOrDefault<Currency> GetByCode(string code);
}
