using ODK.Core.Venues;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;

public interface IVenueLocationRepository : IWriteRepository<VenueLocation>
{
    IDeferredQuerySingleOrDefault<VenueLocation> GetByVenueId(Guid venueId);

    IDeferredQueryMultiple<VenueLocation> GetByVenueIds(IEnumerable<Guid> venueIds);
}
