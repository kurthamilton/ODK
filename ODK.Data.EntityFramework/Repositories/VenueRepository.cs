using Microsoft.EntityFrameworkCore;
using ODK.Core.Venues;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.QueryBuilders;
using ODK.Data.Core.Venues;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.QueryBuilders;

namespace ODK.Data.EntityFramework.Repositories;

public class VenueRepository : ReadWriteRepositoryBase<Venue, IVenueQueryBuilder>, IVenueRepository
{
    public VenueRepository(DbContext context)
        : base(context)
    {
    }

    public IDeferredQuerySingleOrDefault<VenueWithLocationDto> GetLatestByExternalLocationId(string externalId)
        => Query(x => x.ForExternalLocationId(externalId))
            .WithLocation()
            .OrderByDescending(x => x.Venue.CreatedUtc)
            /* Then by id, because every venue predating CreatedUtc carries the moment its migration ran,
               and ids ascend in creation order - so a tie among those still resolves to the latest. */
            .ThenByDescending(x => x.Venue.Id)
            .GetSingleOrDefault();

    public override IVenueQueryBuilder Query()
        => CreateQueryBuilder(context => new VenueQueryBuilder(context));
}