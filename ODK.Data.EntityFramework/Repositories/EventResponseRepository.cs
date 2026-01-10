using ODK.Core.Events;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Events;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class EventResponseRepository : WriteRepositoryBase<EventResponse>, IEventResponseRepository
{
    public EventResponseRepository(OdkContext context)
        : base(context)
    {
    }

    public IDeferredQueryMultiple<EventResponse> GetAllByMemberId(Guid memberId, Guid chapterId) => Query(chapterId)
        .Where(x => x.MemberId == memberId)
        .DeferredMultiple();

    public IDeferredQueryMultiple<EventResponse> GetByChapterId(Guid chapterId) => Query(chapterId)
        .DeferredMultiple();

    public IDeferredQueryMultiple<EventResponse> GetByChapterId(Guid chapterId, IEnumerable<Guid> eventIds) => Query(chapterId)
        .Where(x => eventIds.Contains(x.EventId))
        .DeferredMultiple();

    public IDeferredQueryMultiple<EventResponse> GetByEventId(Guid eventId) => Set()
        .Where(x => x.EventId == eventId)
        .DeferredMultiple();

    public IDeferredQueryMultiple<EventResponse> GetByEventIds(IEnumerable<Guid> eventIds) => Set()
        .Where(x => eventIds.Contains(x.EventId))
        .DeferredMultiple();

    public IDeferredQueryMultiple<EventResponse> GetByMemberId(Guid memberId, DateTime? afterUtc)
    {
        var query =
            from @event in Set<Event>()
            from eventResponse in Set()
            where eventResponse.MemberId == memberId
                && eventResponse.EventId == @event.Id
                && (afterUtc == null || @event.Date >= afterUtc)
            select eventResponse;
        return query.DeferredMultiple();
    }

    public IDeferredQuerySingleOrDefault<EventResponse> GetByMemberId(Guid memberId, Guid eventId) => Set()
        .Where(x => x.MemberId == memberId && x.EventId == eventId)
        .DeferredSingleOrDefault();

    public IDeferredQueryMultiple<EventResponse> GetByMemberId(Guid memberId, IEnumerable<Guid> eventIds) => Set()
        .Where(x => x.MemberId == memberId && eventIds.Contains(x.EventId))
        .DeferredMultiple();

    public IDeferredQuery<int> GetNumberOfAttendees(Guid eventId) => Set()
        .Where(x => x.EventId == eventId && x.Type == EventResponseType.Yes)
        .DeferredCount();

    public IDeferredQueryMultiple<EventResponseSummaryDto> GetResponseSummaries(IEnumerable<Guid> eventIds) => Set()
        .Where(x => eventIds.Contains(x.EventId))
        .GroupBy(x => x.EventId)
        .Select(x => new EventResponseSummaryDto
        {
            EventId = x.Key,
            Yes = x.Count(x => x.Type == EventResponseType.Yes),
            Maybe = x.Count(x => x.Type == EventResponseType.Maybe),
            No = x.Count(x => x.Type == EventResponseType.No)
        })
        .DeferredMultiple();

    private IQueryable<EventResponse> Query(Guid chapterId)
    {
        var query =
            from @event in Set<Event>()
            from eventResponse in Set()
            where @event.ChapterId == chapterId
                && eventResponse.EventId == @event.Id
            select eventResponse;
        return query;
    }
}
