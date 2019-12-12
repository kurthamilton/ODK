using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ODK.Core.Events
{
    public interface IEventRepository
    {
        Task<Event> CreateEvent(Event @event);
        Task DeleteEvent(Guid id);
        Task<IReadOnlyCollection<EventMemberResponse>> GetChapterResponses(Guid chapterId);
        Task<Event> GetEvent(Guid id);
        Task<IReadOnlyCollection<EventMemberResponse>> GetEventResponses(Guid eventId);
        Task<IReadOnlyCollection<Event>> GetEvents(Guid chapterId, DateTime? after);
        Task<IReadOnlyCollection<Event>> GetEventsByVenue(Guid venueId);
        Task<IReadOnlyCollection<EventMemberResponse>> GetMemberResponses(Guid memberId);
        Task<IReadOnlyCollection<Event>> GetPublicEvents(Guid chapterId, DateTime after);
        Task UpdateEvent(Event @event);
        Task UpdateEventResponse(EventMemberResponse response);
    }
}