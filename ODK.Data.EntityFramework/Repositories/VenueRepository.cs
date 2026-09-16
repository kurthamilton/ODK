using Microsoft.EntityFrameworkCore;
using ODK.Core.Venues;
using ODK.Data.Core.QueryBuilders;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.QueryBuilders;

namespace ODK.Data.EntityFramework.Repositories;

public class VenueRepository : ReadWriteRepositoryBase<Venue, IVenueQueryBuilder>, IVenueRepository
{
    public VenueRepository(DbContext context)
        : base(context)
    {
    }

    public override IVenueQueryBuilder Query()
        => CreateQueryBuilder(context => new VenueQueryBuilder(context));
}