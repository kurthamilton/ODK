using Microsoft.EntityFrameworkCore;
using ODK.Core.Venues;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.QueryBuilders;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;
using ODK.Data.EntityFramework.QueryBuilders;

namespace ODK.Data.EntityFramework.Repositories;

public class VenueRepository : ReadWriteRepositoryBase<Venue, IVenueQueryBuilder>, IVenueRepository
{
    public VenueRepository(DbContext context)
        : base(context)
    {
    }

    public IDeferredQueryMultiple<Venue> GetByChapterId(Guid chapterId)
        => Query()
            .ForChapter(chapterId)
            .GetAll();

    public IDeferredQuerySingleOrDefault<Venue> GetByName(Guid chapterId, string name)
        => Set()
            .Where(x => x.ChapterId == chapterId && x.Name == name)
            .DeferredSingleOrDefault();

    public override IVenueQueryBuilder Query()
        => CreateQueryBuilder(context => new VenueQueryBuilder(context));
}