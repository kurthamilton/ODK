using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ODK.Core.Events
{
    public interface IEventRepository
    {
        Task AddEventEmail(EventEmail eventEmail);
        Task<Event> CreateEvent(Event @event);
        Task DeleteEvent(Guid id);
        Task<IReadOnlyCollection<EventMemberResponse>> GetChapterResponses(Guid chapterId);
        Task<Event> GetEvent(Guid id);
        Task<int> GetEventCount(Guid chapterId);
        Task<EventEmail> GetEventEmail(Guid eventId);
        Task<IReadOnlyCollection<EventEmail>> GetEventEmails(Guid chapterId, DateTime after);
        Task<IReadOnlyCollection<EventMemberResponse>> GetEventResponses(Guid eventId);
        Task<IReadOnlyCollection<Event>> GetEvents(Guid chapterId, DateTime after);
        Task<IReadOnlyCollection<Event>> GetEvents(Guid chapterId, int page, int pageSize);
        Task<IReadOnlyCollection<Event>> GetEventsByVenue(Guid venueId);
        Task<IReadOnlyCollection<EventMemberResponse>> GetMemberResponses(Guid memberId);
        Task<IReadOnlyCollection<Event>> GetPublicEvents(Guid chapterId, DateTime after);
        Task UpdateEvent(Event @event);
        Task UpdateEventEmail(EventEmail eventEmail);
        Task UpdateEventResponse(EventMemberResponse response);
    }
}