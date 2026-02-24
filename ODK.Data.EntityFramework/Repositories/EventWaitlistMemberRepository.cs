using Microsoft.EntityFrameworkCore;
using ODK.Core.Events;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class EventWaitlistMemberRepository : ReadWriteRepositoryBase<EventWaitlistMember>, IEventWaitlistMemberRepository
{
    public EventWaitlistMemberRepository(DbContext context)
        : base(context)
    {
    }

    public IDeferredQueryMultiple<EventWaitlistMember> GetByEventId(Guid eventId)
        => Set()
            .Where(x => x.EventId == eventId)
            .DeferredMultiple();

    public IDeferredQueryMultiple<EventWaitlistMember> GetByEventShortcode(string shortcode)
    {
        var query =
            from waitlistMember in Set()
            from @event in Set<Event>()
                .Where(x => x.Id == waitlistMember.EventId)
            where @event.Shortcode == shortcode
            select waitlistMember;

        return query.DeferredMultiple();
    }

    public IDeferredQuerySingleOrDefault<EventWaitlistMember> GetByMemberId(Guid memberId, Guid eventId)
        => Set()
            .Where(x => x.EventId == eventId && x.MemberId == memberId)
            .DeferredSingleOrDefault();

    public IDeferredQuery<bool> IsOnWaitlist(Guid memberId, Guid eventId)
        => Set()
            .Where(x => x.EventId == eventId && x.MemberId == memberId)
            .DeferredAny();
}