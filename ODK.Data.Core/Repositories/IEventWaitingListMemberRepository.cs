using ODK.Core.Events;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;

public interface IEventWaitingListMemberRepository : IReadWriteRepository<EventWaitingListMember>
{
    IDeferredQueryMultiple<EventWaitingListMember> GetByEventId(Guid eventId);

    IDeferredQuerySingleOrDefault<EventWaitingListMember> GetByMemberId(Guid memberId, Guid eventId);

    IDeferredQuery<bool> IsOnWaitingList(Guid memberId, Guid eventId);
}