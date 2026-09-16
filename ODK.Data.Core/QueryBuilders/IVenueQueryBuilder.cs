using ODK.Core.Venues;
using ODK.Data.Core.Venues;

namespace ODK.Data.Core.QueryBuilders;

public interface IVenueQueryBuilder : IDatabaseEntityQueryBuilder<Venue, IVenueQueryBuilder>
{
    IVenueQueryBuilder SlugStartingWith(string prefix);

    IQueryBuilder<VenueWithEventSummaryDto> WithEventSummary();
}