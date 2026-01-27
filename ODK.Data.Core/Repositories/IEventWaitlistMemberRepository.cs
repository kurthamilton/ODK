using ODK.Core.Events;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;

public interface IEventWaitlistMemberRepository : IReadWriteRepository<EventWaitlistMember>
{
    IDeferredQueryMultiple<EventWaitlistMember> GetByEventId(Guid eventId);

    IDeferredQuerySingleOrDefault<EventWaitlistMember> GetByMemberId(Guid memberId, Guid eventId);

    IDeferredQuery<bool> IsOnWaitlist(Guid memberId, Guid eventId);
}