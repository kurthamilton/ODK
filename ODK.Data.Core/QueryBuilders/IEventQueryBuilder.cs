using ODK.Core.Events;
using ODK.Data.Core.Events;

namespace ODK.Data.Core.QueryBuilders;

public interface IEventQueryBuilder : IDatabaseEntityQueryBuilder<Event, IEventQueryBuilder>
{
    IEventQueryBuilder After(DateTime date);

    IEventQueryBuilder Before(DateTime date);

    IEventQueryBuilder Filter(EventAdminFilter filter);

    IEventQueryBuilder ForChapter(Guid chapterId);

    IEventQueryBuilder ForShortcode(string shortcode);

    IEventQueryBuilder ForVenue(Guid venueId);

    IEventQueryBuilder OnOrAfter(DateTime date);

    IEventQueryBuilder Past();

    IEventQueryBuilder Public();

    IEventQueryBuilder Published();

    IQueryBuilder<EventSummaryDto> Summary();

    IVenueQueryBuilder Venue();

    IEventWithLocalDateQueryBuilder WithLocalDate();

    IQueryBuilder<EventWithVenueDto> WithVenue();
}