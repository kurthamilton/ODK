using Microsoft.EntityFrameworkCore;
using ODK.Core.Venues;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class VenueLocationRepository : WriteRepositoryBase<VenueLocation>, IVenueLocationRepository
{
    public VenueLocationRepository(DbContext context)
        : base(context)
    {
    }

    public IDeferredQuerySingleOrDefault<VenueLocation> GetByVenueId(Guid venueId)
        => Set()
            .Where(x => x.VenueId == venueId)
            .DeferredSingleOrDefault();

    public IDeferredQueryMultiple<VenueLocation> GetByVenueIds(IEnumerable<Guid> venueIds)
        => Set()
            .Where(x => venueIds.Contains(x.VenueId))
            .DeferredMultiple();
}
