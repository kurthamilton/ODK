using ODK.Core.Events;

namespace ODK.Services.Events;

public interface IEventAdminService
{
    Task<ServiceResult> CreateEvent(AdminServiceRequest request, CreateEvent createEvent, bool draft);

    Task DeleteEvent(AdminServiceRequest request, Guid id);
    
    Task<IReadOnlyCollection<EventInvitesDto>> GetChapterInvites(AdminServiceRequest request, 
        IReadOnlyCollection<Guid> eventIds);
    
    Task<IReadOnlyCollection<EventResponse>> GetChapterResponses(AdminServiceRequest request, 
        IReadOnlyCollection<Guid> eventIds);

    Task<Event> GetEvent(AdminServiceRequest request, Guid id);

    Task<IReadOnlyCollection<EventHost>> GetEventHosts(AdminServiceRequest request, Guid eventId);

    Task<EventInvitesDto> GetEventInvites(AdminServiceRequest request, Guid eventId);

    Task<EventResponsesDto> GetEventResponsesDto(AdminServiceRequest request, Guid eventId);

    Task<IReadOnlyCollection<Event>> GetEvents(AdminServiceRequest request, int page, int pageSize);    

    Task<EventsDto> GetEventsDto(AdminServiceRequest request, int page, int pageSize);

    Task<IReadOnlyCollection<Event>> GetEventsByVenue(AdminServiceRequest request, Guid venueId);

    Task<IReadOnlyCollection<EventTicketPurchase>> GetEventTicketPurchases(AdminServiceRequest request, Guid eventId);

    Task<DateTime> GetNextAvailableEventDate(AdminServiceRequest request);

    Task PublishEvent(AdminServiceRequest request, Guid eventId);

    Task SendEventInviteeEmail(AdminServiceRequest request, Guid eventId, 
        IEnumerable<EventResponseType> responseTypes, string subject, string body);

    Task<ServiceResult> SendEventInvites(AdminServiceRequest request, Guid eventId, bool test = false);

    Task SendScheduledEmails();

    Task<ServiceResult> UpdateEvent(AdminServiceRequest request, Guid id, CreateEvent @event);

    Task<EventResponse> UpdateMemberResponse(AdminServiceRequest request, Guid eventId, Guid memberId, 
        EventResponseType responseType);

    Task<ServiceResult> UpdateScheduledEmail(AdminServiceRequest request, Guid eventId, DateTime? date, TimeSpan? time);
}
