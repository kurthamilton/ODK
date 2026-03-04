using ODK.Core.Events;
using ODK.Data.Core.Events;

namespace ODK.Data.Core.QueryBuilders;

public interface IEventQueryBuilder : IDatabaseEntityQueryBuilder<Event, IEventQueryBuilder>
{
    IEventQueryBuilder After(DateTime date);

    IEventQueryBuilder ForChapter(Guid chapterId);

    IEventQueryBuilder ForShortcode(string shortcode);

    IEventQueryBuilder ForVenue(Guid venueId);

    IEventQueryBuilder Past();

    IEventQueryBuilder Public();

    IEventQueryBuilder Published();

    IQueryBuilder<EventSummaryDto> Summary();

    IVenueQueryBuilder Venue();

    IQueryBuilder<EventWithVenueDto> WithVenue();
}