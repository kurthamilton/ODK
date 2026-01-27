using ODK.Core.Events;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class EventWaitlistMemberRepository : ReadWriteRepositoryBase<EventWaitlistMember>, IEventWaitlistMemberRepository
{
    public EventWaitlistMemberRepository(OdkContext context)
        : base(context)
    {
    }

    public IDeferredQueryMultiple<EventWaitlistMember> GetByEventId(Guid eventId)
        => Set()
            .Where(x => x.EventId == eventId)
            .DeferredMultiple();

    public IDeferredQuerySingleOrDefault<EventWaitlistMember> GetByMemberId(Guid memberId, Guid eventId)
        => Set()
            .Where(x => x.EventId == eventId && x.MemberId == memberId)
            .DeferredSingleOrDefault();

    public IDeferredQuery<bool> IsOnWaitlist(Guid memberId, Guid eventId)
        => Set()
            .Where(x => x.EventId == eventId && x.MemberId == memberId)
            .DeferredAny();
}