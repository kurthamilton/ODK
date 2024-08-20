using Microsoft.EntityFrameworkCore;
using ODK.Core.Venues;
using ODK.Data.Core.Repositories;

namespace ODK.Data.EntityFramework.Repositories;

public class VenueLocationRepository : WriteRepositoryBase<VenueLocation>, IVenueLocationRepository
{
    public VenueLocationRepository(OdkContext context) 
        : base(context)
    {
    }

    public Task<VenueLocation?> GetByVenueId(Guid venueId) => Set()
        .Where(x => x.VenueId == venueId)
        .FirstOrDefaultAsync();

    public async Task<IReadOnlyCollection<VenueLocation>> GetByVenueIds(IEnumerable<Guid> venueIds) => await Set()
        .Where(x => venueIds.Contains(x.VenueId))
        .ToArrayAsync();
}
