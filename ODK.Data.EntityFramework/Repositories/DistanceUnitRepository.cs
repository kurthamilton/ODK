using ODK.Core.Countries;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class DistanceUnitRepository : ReadWriteRepositoryBase<DistanceUnit>, IDistanceUnitRepository
{
    public DistanceUnitRepository(OdkContext context)
        : base(context)
    {
    }

    public IDeferredQueryMultiple<DistanceUnit> GetAll()
        => Set()
            .DeferredMultiple();

    public IDeferredQuerySingle<DistanceUnit> GetByType(DistanceUnitType type)
        => Set()
            .Where(x => x.Type == type)
            .DeferredSingle();
}