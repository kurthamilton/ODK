using ODK.Core.Events;
using ODK.Data.Core.Deferred;

namespace ODK.Data.Core.Repositories;

public interface IEventRepository : IReadWriteRepository<Event>
{
    IDeferredQueryMultiple<Event> GetByChapterId(Guid chapterId);
    IDeferredQueryMultiple<Event> GetByChapterId(Guid chapterId, DateTime? after);
    IDeferredQueryMultiple<Event> GetByChapterId(Guid chapterId, int page, int pageSize);
    IDeferredQueryMultiple<Event> GetByVenueId(Guid venueId);    
    IDeferredQueryMultiple<Event> GetPublicEventsByChapterId(Guid chapterId, DateTime? after);
    IDeferredQueryMultiple<Event> GetRecentEventsByChapterId(Guid chapterId, int pageSize);
}