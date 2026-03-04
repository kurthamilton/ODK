using ODK.Core.Venues;
using ODK.Data.Core.Venues;

namespace ODK.Data.Core.QueryBuilders;

public interface IVenueQueryBuilder : IDatabaseEntityQueryBuilder<Venue, IVenueQueryBuilder>
{
    IVenueQueryBuilder Archived(bool value);

    IVenueQueryBuilder ForChapter(Guid chapterId);

    IQueryBuilder<VenueWithEventSummaryDto> WithEventSummary();
}