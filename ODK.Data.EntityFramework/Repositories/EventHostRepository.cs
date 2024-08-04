using Microsoft.EntityFrameworkCore;
using ODK.Core.Events;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class EventHostRepository : ReadWriteRepositoryBase<EventHost>, IEventHostRepository
{
    public EventHostRepository(OdkContext context) 
        : base(context)
    {
    }

    public IDeferredQueryMultiple<EventHost> GetByEventId(Guid eventId) => Set()
        .Where(x => x.EventId == eventId)
        .DeferredMultiple();

    protected override IQueryable<EventHost> Set() => base.Set()
        .Include(x => x.Member);
}
