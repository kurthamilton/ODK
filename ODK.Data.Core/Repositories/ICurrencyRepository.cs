using ODK.Core.Countries;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;

public interface ICurrencyRepository : IReadWriteRepository<Currency>
{
    IDeferredQueryMultiple<Currency> GetAll();

    IDeferredQuerySingleOrDefault<Currency> GetByCode(string code);

    IDeferredQuerySingle<Currency> GetByChapterId(Guid chapterId);

    IDeferredQuerySingleOrDefault<Currency> GetByChapterIdOrDefault(Guid chapterId);

    IDeferredQuerySingle<Currency> GetByCountryId(Guid countryId);

    IDeferredQuerySingleOrDefault<Currency> GetByMemberIdOrDefault(Guid memberId);
}