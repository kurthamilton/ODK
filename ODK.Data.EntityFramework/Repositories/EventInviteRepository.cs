using ODK.Core.Events;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;
public class EventInviteRepository : WriteRepositoryBase<EventInvite>, IEventInviteRepository
{
    public EventInviteRepository(OdkContext context) 
        : base(context)
    {
    }

    public IDeferredQueryMultiple<EventInvite> GetByEventId(Guid eventId) => Set()
        .Where(x => x.EventId == eventId)
        .DeferredMultiple();

    public IDeferredQueryMultiple<EventInvite> GetByEventIds(IEnumerable<Guid> eventIds) => Set()
        .Where(x => eventIds.Contains(x.EventId))
        .DeferredMultiple();

    public IDeferredQueryMultiple<EventInvite> GetByMemberId(Guid memberId) => Set()
        .Where(x => x.MemberId == memberId)
        .DeferredMultiple();

    public IDeferredQueryMultiple<EventInvite> GetChapterInvites(Guid chapterId, IEnumerable<Guid> eventIds)
    {
        var query = 
            from @event in Set<Event>()
            from eventInvite in Set()
            where @event.ChapterId == chapterId
                && eventInvite.EventId == @event.Id
                && eventIds.Contains(@event.Id)
            select eventInvite;
        return query.DeferredMultiple();
    }

    public IDeferredQueryMultiple<EventInviteSummaryDto> GetEventInvitesDtos(IEnumerable<Guid> eventIds) => Set()
        .Where(x => eventIds.Contains(x.EventId))
        .GroupBy(x => x.EventId)
        .Select(x => new EventInviteSummaryDto
        {
            EventId = x.Key,
            Sent = x.Count()
        })
        .DeferredMultiple();
}
