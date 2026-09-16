using Microsoft.EntityFrameworkCore;
using ODK.Core.Venues;
using ODK.Data.Core.QueryBuilders;

namespace ODK.Data.EntityFramework.QueryBuilders;

public class ChapterVenueQueryBuilder
    : QueryBuilder<ChapterVenue>, IChapterVenueQueryBuilder
{
    public ChapterVenueQueryBuilder(DbContext context)
        : this(context, context.Set<ChapterVenue>().Include(x => x.Venue))
    {
    }

    public ChapterVenueQueryBuilder(DbContext context, IQueryable<ChapterVenue> query)
        : base(context, query)
    {
    }

    public IChapterVenueQueryBuilder Archived(bool value)
    {
        Query = Query.Where(x => x.ArchivedUtc != null == value);
        return this;
    }

    public IChapterVenueQueryBuilder ForChapter(Guid chapterId)
    {
        Query = Query.Where(x => x.ChapterId == chapterId);
        return this;
    }

    public IChapterVenueQueryBuilder ForVenue(Guid venueId)
    {
        Query = Query.Where(x => x.VenueId == venueId);
        return this;
    }

    public IVenueQueryBuilder ToVenue() =>
        CreateQueryBuilder<IVenueQueryBuilder, Venue>(context =>
            new VenueQueryBuilder(context, Query.Select(x => x.Venue)));
}