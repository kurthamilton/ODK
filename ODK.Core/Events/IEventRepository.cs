using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ODK.Core.Events
{
    public interface IEventRepository
    {
        Task<Event> CreateEvent(Event @event);
        Task<Event> GetEvent(Guid id);
        Task<IReadOnlyCollection<Event>> GetEvents(Guid chapterId, DateTime? after);
        Task<IReadOnlyCollection<EventMemberResponse>> GetEventResponses(Guid eventId);
        Task<IReadOnlyCollection<Event>> GetPublicEvents(Guid chapterId, DateTime after);
        Task UpdateEventResponse(EventMemberResponse response);
    }
}