using ODK.Core.Events;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Events;

namespace ODK.Data.Core.Repositories;

public interface IEventCommentRepository : IReadWriteRepository<EventComment>
{
    IDeferredQueryMultiple<EventComment> GetByEventId(Guid eventId);

    IDeferredQueryMultiple<EventComment> GetByEventShortcode(string shortcode);

    IDeferredQuerySingle<EventComment> GetById(Guid id, bool includeHidden);

    IDeferredQueryMultiple<EventCommentDto> GetDtosByEventId(Guid eventId, bool includeHidden);
}