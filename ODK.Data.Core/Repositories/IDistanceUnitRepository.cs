using ODK.Core.Countries;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;

public interface IDistanceUnitRepository : IReadWriteRepository<DistanceUnit>
{
    IDeferredQueryMultiple<DistanceUnit> GetAll();

    IDeferredQuerySingle<DistanceUnit> GetByType(DistanceUnitType type);
}