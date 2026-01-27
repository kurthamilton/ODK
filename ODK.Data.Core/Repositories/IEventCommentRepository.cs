using ODK.Core.Events;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;

public interface IEventCommentRepository : IReadWriteRepository<EventComment>
{
    IDeferredQueryMultiple<EventComment> GetByEventId(Guid eventId);

    IDeferredQueryMultiple<EventComment> GetByEventShortcode(string shortcode);
}