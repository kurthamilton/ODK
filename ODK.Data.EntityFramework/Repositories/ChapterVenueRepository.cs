using Microsoft.EntityFrameworkCore;
using ODK.Core.Venues;
using ODK.Data.Core.QueryBuilders;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.QueryBuilders;

namespace ODK.Data.EntityFramework.Repositories;

public class ChapterVenueRepository
    : WriteRepositoryBase<ChapterVenue, IChapterVenueQueryBuilder>, IChapterVenueRepository
{
    public ChapterVenueRepository(DbContext context)
        : base(context)
    {
    }

    public override IChapterVenueQueryBuilder Query()
        => CreateQueryBuilder<IChapterVenueQueryBuilder>(context => new ChapterVenueQueryBuilder(context));

    protected override IQueryable<ChapterVenue> Set()
        => base.Set().Include(x => x.Venue);
}
