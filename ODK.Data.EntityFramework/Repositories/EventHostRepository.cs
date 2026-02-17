using Microsoft.EntityFrameworkCore;
using ODK.Core.Events;
using ODK.Core.Members;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Members;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class EventHostRepository : ReadWriteRepositoryBase<EventHost>, IEventHostRepository
{
    public EventHostRepository(OdkContext context)
        : base(context)
    {
    }

    public IDeferredQueryMultiple<EventHost> GetByEventId(Guid eventId)
        => Set()
            .Where(x => x.EventId == eventId)
            .DeferredMultiple();

    public IDeferredQueryMultiple<MemberWithAvatarDto> GetByEventShortcode(string shortcode)
    {
        var query =
            from eventHost in Set()
            from @event in Set<Event>()
                .Where(x => x.Id == eventHost.EventId)
            from avatar in Set<MemberAvatar>()
                .Where(x => x.MemberId == eventHost.Member.Id)
                .DefaultIfEmpty()
            where @event.Shortcode == shortcode
            select new MemberWithAvatarDto
            {
                AvatarVersion = avatar != null ? avatar.VersionInt : null,
                Member = eventHost.Member
            };

        return query.DeferredMultiple();
    }

    protected override IQueryable<EventHost> Set() 
        => base.Set()
            .Include(x => x.Member);
}