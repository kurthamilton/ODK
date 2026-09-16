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

    /* Matched under the database's collation, which is case-insensitive, so the candidates this
       collects are the same set CreateSlug then compares case-insensitively. */
    public IVenueQueryBuilder SlugStartingWith(string prefix)
    {
        Query = Query.Where(x => x.Slug.StartsWith(prefix));
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
                    .OrderByDescending(x => x.DateUtc)
                    .FirstOrDefault(),
                Venue = venue
            };
        return ProjectTo(query);
    }
}