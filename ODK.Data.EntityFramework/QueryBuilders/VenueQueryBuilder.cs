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

    /* The place a venue records is identified on its location, not on the venue, so this reaches through
       VenueLocations. Several venues can match: they are that place as it was at different times. */
    public IVenueQueryBuilder ForExternalLocationId(string externalId)
    {
        Query = Query
            .Where(venue => Set<VenueLocation>()
                .Any(x => x.VenueId == venue.Id && x.ExternalId == externalId));
        return this;
    }

    /* Matched under the database's collation, which is case-insensitive, so the candidates this
       collects are the same set CreateSlug then compares case-insensitively. */
    public IVenueQueryBuilder SlugStartingWith(string prefix)
    {
        Query = Query.Where(x => x.Slug.StartsWith(prefix));
        return this;
    }

    public IQueryBuilder<VenueWithLocationDto> WithLocation()
    {
        var query =
            from venue in Query
            from location in Set<VenueLocation>()
                .Where(x => x.VenueId == venue.Id)
                .DefaultIfEmpty()
            select new VenueWithLocationDto
            {
                Location = location,
                Venue = venue
            };
        return ProjectTo(query);
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