namespace ODK.Core.Events;

public interface IEventRepository
{
    Task<Guid> AddEventEmailAsync(EventEmail eventEmail);
    Task AddEventInvitesAsync(Guid eventId, IEnumerable<Guid> memberIds, DateTime sentDate);
    Task<Event> CreateEventAsync(Event @event);
    Task DeleteEventAsync(Guid id);
    Task<IReadOnlyCollection<EventInvite>> GetChapterInvitesAsync(Guid chapterId, IEnumerable<Guid> eventIds);
    Task<IReadOnlyCollection<EventResponse>> GetChapterResponsesAsync(Guid chapterId);
    Task<IReadOnlyCollection<EventResponse>> GetChapterResponsesAsync(Guid chapterId, IEnumerable<Guid> eventIds);
    Task<Event?> GetEventAsync(Guid id);
    Task<EventEmail?> GetEventEmailAsync(Guid eventId);
    Task<IReadOnlyCollection<EventEmail>> GetEventEmailsAsync(Guid chapterId, IEnumerable<Guid> eventIds);
    Task<IReadOnlyCollection<EventInvite>> GetEventInvitesAsync(Guid eventId);
    Task<IReadOnlyCollection<EventInvite>> GetEventInvitesForMemberIdAsync(Guid memberId);
    Task<IReadOnlyCollection<EventResponse>> GetEventResponsesAsync(Guid eventId);
    Task<IReadOnlyCollection<Event>> GetEventsAsync(Guid chapterId, DateTime? after);
    Task<IReadOnlyCollection<Event>> GetEventsAsync(Guid chapterId, int page, int pageSize);
    Task<IReadOnlyCollection<Event>> GetEventsByVenueAsync(Guid venueId);
    Task<IReadOnlyCollection<EventResponse>> GetMemberResponsesAsync(Guid memberId, bool allEvents = false);
    Task<IReadOnlyCollection<Event>> GetPublicEventsAsync(Guid chapterId, DateTime? after);
    Task UpdateEventAsync(Event @event);
    Task UpdateEventResponseAsync(EventResponse response);
}