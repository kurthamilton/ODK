using ODK.Core.Events;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Events;
using ODK.Data.Core.QueryBuilders;

namespace ODK.Data.Core.Repositories;

public interface IEventRepository : IReadWriteRepository<Event, IEventQueryBuilder>
{
    IDeferredQueryMultiple<Event> GetByChapterId(Guid chapterId);

    IDeferredQueryMultiple<Event> GetByChapterId(Guid chapterId, DateTime after);

    IDeferredQuerySingle<Event> GetByShortcode(string shortcode);

    /// <summary>
    /// The group's earliest created event, or null where it has none. The checklist reads its
    /// <see cref="Event.CreatedUtc"/>, dating the step by when it actually happened rather than by when
    /// the dashboard noticed.
    /// </summary>
    IDeferredQuerySingleOrDefault<Event> GetFirstCreatedByChapterId(Guid chapterId);

    IDeferredQuery<int> GetCountByChapterId(Guid chapterId, string? venueSlug, DateTime? fromUtc, DateTime? toUtcExclusive);

    IDeferredQuery<int> GetPastEventCountByChapterId(Guid chapterId);

    IDeferredQueryMultiple<Event> GetRecentEventsByChapterId(Guid chapterId, int pageSize);

    IDeferredQueryMultiple<EventSummaryDto> GetSummariesByChapterId(
        Guid chapterId, string? venueSlug, DateTime? fromUtc, DateTime? toUtcExclusive, PageFilter pageFilter);

    IDeferredQueryMultiple<Event> GetUpcoming(Guid chapterId, int pageSize);

    IDeferredQuery<bool> ShortcodeExists(string shortcode);
}