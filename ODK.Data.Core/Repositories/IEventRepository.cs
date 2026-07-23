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

    IDeferredQueryMultiple<Event> GetByVenueId(Guid venueId);

    IDeferredQuery<int> GetCountByChapterId(Guid chapterId, EventAdminFilter filter);

    IDeferredQuery<int> GetPastEventCountByChapterId(Guid chapterId);

    IDeferredQueryMultiple<Event> GetRecentEventsByChapterId(Guid chapterId, int pageSize);

    IDeferredQueryMultiple<EventSummaryDto> GetSummariesByChapterId(
        Guid chapterId, EventAdminFilter filter, PageFilter pageFilter);

    IDeferredQueryMultiple<Event> GetUpcoming(Guid chapterId, int pageSize);

    IDeferredQuery<bool> ShortcodeExists(string shortcode);
}