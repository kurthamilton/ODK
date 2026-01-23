using ODK.Core.Events;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class EventWaitingListMemberRepository : ReadWriteRepositoryBase<EventWaitingListMember>, IEventWaitingListMemberRepository
{
    public EventWaitingListMemberRepository(OdkContext context)
        : base(context)
    {
    }

    public IDeferredQueryMultiple<EventWaitingListMember> GetByEventId(Guid eventId)
        => Set()
            .Where(x => x.EventId == eventId)
            .DeferredMultiple();

    public IDeferredQuerySingleOrDefault<EventWaitingListMember> GetByMemberId(Guid memberId, Guid eventId)
        => Set()
            .Where(x => x.EventId == eventId && x.MemberId == memberId)
            .DeferredSingleOrDefault();

    public IDeferredQuery<bool> IsOnWaitingList(Guid memberId, Guid eventId)
        => Set()
            .Where(x => x.EventId == eventId && x.MemberId == memberId)
            .DeferredAny();
}