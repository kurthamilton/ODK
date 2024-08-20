using ODK.Core.Venues;

namespace ODK.Data.Core.Repositories;

public interface IVenueLocationRepository : IWriteRepository<VenueLocation>
{
    Task<VenueLocation?> GetByVenueId(Guid venueId);

    Task<IReadOnlyCollection<VenueLocation>> GetByVenueIds(IEnumerable<Guid> venueIds);
}
