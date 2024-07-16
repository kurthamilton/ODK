using ODK.Core.Countries;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;

public interface ICountryRepository : IReadWriteRepository<Country>
{
    IDeferredQuerySingle<Country> GetByChapterId(Guid chapterId);
    IDeferredQueryMultiple<Country> GetAll();
}
