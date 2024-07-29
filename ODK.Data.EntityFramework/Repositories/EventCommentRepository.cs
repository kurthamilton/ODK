using ODK.Core.Events;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class EventCommentRepository : ReadWriteRepositoryBase<EventComment>, IEventCommentRepository
{
    public EventCommentRepository(OdkContext context) 
        : base(context)
    {
    }

    public IDeferredQueryMultiple<EventComment> GetByEventId(Guid eventId) => Set()
        .Where(x => x.EventId == eventId)
        .OrderBy(x => x.CreatedUtc)
        .DeferredMultiple();

    protected override IQueryable<EventComment> Set() => base.Set()
        .Where(x => !x.Hidden);
}
