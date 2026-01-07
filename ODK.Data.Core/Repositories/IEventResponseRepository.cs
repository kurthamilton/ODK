using ODK.Core.Events;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;

public interface IEventResponseRepository : IWriteRepository<EventResponse>
{
    IDeferredQueryMultiple<EventResponse> GetAllByMemberId(Guid memberId, Guid chapterId);
    IDeferredQueryMultiple<EventResponse> GetByChapterId(Guid chapterId);
    IDeferredQueryMultiple<EventResponse> GetByChapterId(Guid chapterId, IEnumerable<Guid> eventIds);
    IDeferredQueryMultiple<EventResponse> GetByEventId(Guid eventId);
    IDeferredQueryMultiple<EventResponse> GetByEventIds(IEnumerable<Guid> eventIds);
    IDeferredQueryMultiple<EventResponse> GetByMemberId(Guid memberId, DateTime? afterUtc);
    IDeferredQuerySingleOrDefault<EventResponse> GetByMemberId(Guid memberId, Guid eventId);
    IDeferredQueryMultiple<EventResponse> GetByMemberId(Guid memberId, IEnumerable<Guid> eventIds);
    IDeferredQuery<int> GetNumberOfAttendees(Guid eventId);
    IDeferredQueryMultiple<EventResponseSummaryDto> GetResponseSummaries(IEnumerable<Guid> eventIds);
}
