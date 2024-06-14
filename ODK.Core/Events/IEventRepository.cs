namespace ODK.Core.Events;

public interface IEventRepository
{
    Task<Guid> AddEventEmail(EventEmail eventEmail);
    Task AddEventInvites(Guid eventId, IEnumerable<Guid> memberIds, DateTime sentDate);
    Task<Event> CreateEvent(Event @event);
    Task DeleteEvent(Guid id);
    Task<IReadOnlyCollection<EventInvite>> GetChapterInvites(Guid chapterId, IEnumerable<Guid> eventIds);
    Task<IReadOnlyCollection<EventResponse>> GetChapterResponses(Guid chapterId);
    Task<IReadOnlyCollection<EventResponse>> GetChapterResponses(Guid chapterId, IEnumerable<Guid> eventIds);
    Task<Event?> GetEvent(Guid id);
    Task<EventEmail?> GetEventEmail(Guid eventId);
    Task<IReadOnlyCollection<EventEmail>> GetEventEmails(Guid chapterId, IEnumerable<Guid> eventIds);
    Task<IReadOnlyCollection<EventInvite>> GetEventInvites(Guid eventId);
    Task<IReadOnlyCollection<EventInvite>> GetEventInvitesForMemberId(Guid memberId);
    Task<IReadOnlyCollection<EventResponse>> GetEventResponses(Guid eventId);
    Task<IReadOnlyCollection<Event>> GetEvents(Guid chapterId, DateTime? after);
    Task<IReadOnlyCollection<Event>> GetEvents(Guid chapterId, int page, int pageSize);
    Task<IReadOnlyCollection<Event>> GetEventsByVenue(Guid venueId);
    Task<IReadOnlyCollection<EventResponse>> GetMemberResponses(Guid memberId, bool allEvents = false);
    Task<IReadOnlyCollection<Event>> GetPublicEvents(Guid chapterId, DateTime? after);
    Task UpdateEvent(Event @event);
    Task UpdateEventResponse(EventResponse response);
}