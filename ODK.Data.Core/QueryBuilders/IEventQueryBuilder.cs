using ODK.Core.Events;
using ODK.Data.Core.Events;

namespace ODK.Data.Core.QueryBuilders;

public interface IEventQueryBuilder : IDatabaseEntityQueryBuilder<Event, IEventQueryBuilder>
{
    IEventQueryBuilder After(DateTime date);

    IEventQueryBuilder Before(DateTime date);

    IEventQueryBuilder ForChapter(Guid chapterId);

    IEventQueryBuilder ForChapters(IEnumerable<Guid> chapterIds);

    IEventQueryBuilder ForShortcode(string shortcode);

    IEventQueryBuilder ForVenue(Guid venueId);

    IEventQueryBuilder ForVenueSlug(string slug);

    IEventQueryBuilder OnOrAfter(DateTime date);

    IEventQueryBuilder Past();

    IEventQueryBuilder Public();

    /// <summary>
    /// Projects to what identifies each event and when it was published. Only meaningful after
    /// <see cref="Published"/> - an unpublished event has no publication date.
    /// </summary>
    IQueryBuilder<EventPublicationDto> Publication();

    IEventQueryBuilder Published();

    IQueryBuilder<EventSummaryDto> Summary();

    IVenueQueryBuilder Venue();

    IQueryBuilder<EventWithVenueDto> WithVenue();
}