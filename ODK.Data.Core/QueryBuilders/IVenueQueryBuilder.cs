using ODK.Core.Venues;
using ODK.Data.Core.Venues;

namespace ODK.Data.Core.QueryBuilders;

public interface IVenueQueryBuilder : IDatabaseEntityQueryBuilder<Venue, IVenueQueryBuilder>
{
    IVenueQueryBuilder ForExternalLocationId(string externalId);

    IVenueQueryBuilder SlugStartingWith(string prefix);

    IQueryBuilder<VenueWithChapterCountDto> WithChapterCount();

    IQueryBuilder<VenueWithLocationDto> WithLocation();
}