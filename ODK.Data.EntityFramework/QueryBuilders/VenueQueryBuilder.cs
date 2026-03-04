using Microsoft.EntityFrameworkCore;
using ODK.Core.Events;
using ODK.Core.Venues;
using ODK.Data.Core.QueryBuilders;
using ODK.Data.Core.Venues;

namespace ODK.Data.EntityFramework.QueryBuilders;

public class VenueQueryBuilder
    : DatabaseEntityQueryBuilder<Venue, IVenueQueryBuilder>, IVenueQueryBuilder
{
    public VenueQueryBuilder(DbContext context)
        : base(context)
    {
    }

    public VenueQueryBuilder(DbContext context, IQueryable<Venue> query)
        : base(context, query)
    {
    }

    protected override IVenueQueryBuilder Builder => this;

    public IVenueQueryBuilder Archived(bool value)
    {
        Query = Query.Where(x => x.ArchivedUtc != null == value);
        return this;
    }

    public IVenueQueryBuilder ForChapter(Guid chapterId)
    {
        Query = Query.Where(x => x.ChapterId == chapterId);
        return this;
    }

    public IQueryBuilder<VenueWithEventSummaryDto> WithEventSummary()
    {
        var query =
            from venue in Query
            select new VenueWithEventSummaryDto
            {
                EventCount = Set<Event>()
                    .Where(x => x.VenueId == venue.Id)
                    .Count(),
                LastEvent = Set<Event>()
                    .Where(x => x.VenueId == venue.Id)
                    .OrderByDescending(x => x.Date)
                    .FirstOrDefault(),
                Venue = venue
            };
        return ProjectTo(query);
    }
}