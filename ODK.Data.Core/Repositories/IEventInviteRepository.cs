using ODK.Core.Events;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;
public interface IEventInviteRepository : IWriteRepository<EventInvite>
{
    IDeferredQueryMultiple<EventInvite> GetByEventId(Guid eventId);
    IDeferredQueryMultiple<EventInvite> GetByEventIds(IEnumerable<Guid> eventIds);
    IDeferredQueryMultiple<EventInvite> GetByMemberId(Guid memberId);
    IDeferredQueryMultiple<EventInvite> GetByMemberId(Guid memberId, IEnumerable<Guid> eventIds);
    IDeferredQueryMultiple<EventInvite> GetChapterInvites(Guid chapterId, IEnumerable<Guid> eventIds);
    IDeferredQueryMultiple<EventInviteSummaryDto> GetEventInvitesDtos(IEnumerable<Guid> eventIds);
}
