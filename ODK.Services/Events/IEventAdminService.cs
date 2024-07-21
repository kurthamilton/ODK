using ODK.Core.Events;

namespace ODK.Services.Events;

public interface IEventAdminService
{
    Task<ServiceResult> CreateEvent(Guid memberId, CreateEvent createEvent);

    Task DeleteEvent(Guid currentMemberId, Guid id);
    
    Task<IReadOnlyCollection<EventInvitesDto>> GetChapterInvites(Guid currentMemberId, Guid chapterId, IReadOnlyCollection<Guid> eventIds);
    
    Task<IReadOnlyCollection<EventResponse>> GetChapterResponses(Guid currentMemberId, Guid chapterId, IReadOnlyCollection<Guid> eventIds);

    Task<Event> GetEvent(Guid currentMemberId, Guid id);
    
    Task<EventInvitesDto> GetEventInvites(Guid currentMemberId, Guid eventId);

    Task<EventResponsesDto> GetEventResponsesDto(Guid currentMemberId, Guid eventId);

    Task<IReadOnlyCollection<Event>> GetEvents(Guid currentMemberId, Guid chapterId, int page, int pageSize);

    Task<EventsDto> GetEventsDto(Guid currentMemberId, Guid chapterId, int page, int pageSize);

    Task<IReadOnlyCollection<Event>> GetEventsByVenue(Guid currentMemberId, Guid venueId);

    Task<DateTime?> GetNextAvailableEventDate(Guid currentMemberId, Guid chapterId);

    Task SendEventInviteeEmail(Guid currentMemberId, Guid eventId, IEnumerable<EventResponseType> responseTypes, 
        string subject, string body);

    Task<ServiceResult> SendEventInvites(Guid currentMemberId, Guid eventId, bool test = false);

    Task SendScheduledEmails();

    Task<ServiceResult> UpdateEvent(Guid memberId, Guid id, CreateEvent @event);

    Task<EventResponse> UpdateMemberResponse(Guid currentMemberId, Guid eventId, Guid memberId, EventResponseType responseType);

    Task<ServiceResult> UpdateScheduledEmail(Guid currentMemberId, Guid eventId, DateTime? date, string? time);
}
