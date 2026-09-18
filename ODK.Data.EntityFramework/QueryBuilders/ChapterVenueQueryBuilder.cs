using Microsoft.EntityFrameworkCore;
using ODK.Core.Events;
using ODK.Core.Venues;
using ODK.Data.Core.QueryBuilders;
using ODK.Data.Core.Venues;

namespace ODK.Data.EntityFramework.QueryBuilders;

public class ChapterVenueQueryBuilder
    : DatabaseEntityQueryBuilder<ChapterVenue, IChapterVenueQueryBuilder>, IChapterVenueQueryBuilder
{
    public ChapterVenueQueryBuilder(DbContext context)
        : this(context, context.Set<ChapterVenue>().Include(x => x.Venue))
    {
    }

    public ChapterVenueQueryBuilder(DbContext context, IQueryable<ChapterVenue> query)
        : base(context, query)
    {
    }

    protected override IChapterVenueQueryBuilder Builder => this;

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

    public IQueryBuilder<ChapterVenueWithEventSummaryDto> WithEventSummary()
    {
        var query =
            from chapterVenue in Query
            from location in Set<VenueLocation>()
                .Where(x => x.VenueId == chapterVenue.VenueId)
                .DefaultIfEmpty()
            select new ChapterVenueWithEventSummaryDto
            {
                ChapterVenue = chapterVenue,
                EventCount = Set<Event>()
                    .Where(x => x.VenueId == chapterVenue.VenueId)
                    .Count(),
                LastEvent = Set<Event>()
                    .Where(x => x.VenueId == chapterVenue.VenueId)
                    .OrderByDescending(x => x.DateUtc)
                    .FirstOrDefault(),
                Location = location
            };

        return ProjectTo(query);
    }

    public IQueryBuilder<ChapterVenueWithLocationDto> WithLocation()
    {
        var query =
            from chapterVenue in Query
            from location in Set<VenueLocation>()
                .Where(x => x.VenueId == chapterVenue.VenueId)
                .DefaultIfEmpty()
            select new ChapterVenueWithLocationDto
            {
                ChapterVenue = chapterVenue,
                Location = location
            };

        return ProjectTo(query);
    }
}