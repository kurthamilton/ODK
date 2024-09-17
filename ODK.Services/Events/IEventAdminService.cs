using ODK.Core.Events;
using ODK.Services.Events.ViewModels;

namespace ODK.Services.Events;

public interface IEventAdminService
{
    Task<ServiceResult> CreateEvent(AdminServiceRequest request, CreateEvent createEvent, bool draft);

    Task DeleteEvent(AdminServiceRequest request, Guid id);
    
    Task<Event> GetEvent(AdminServiceRequest request, Guid id);

    Task<EventAttendeesAdminPageViewModel> GetEventAttendeesViewModel(AdminServiceRequest request, Guid eventId);

    Task<EventCreateAdminPageViewModel> GetEventCreateViewModel(AdminServiceRequest request);

    Task<EventEditAdminPageViewModel> GetEventEditViewModel(AdminServiceRequest request, Guid eventId);

    Task<EventInvitesAdminPageViewModel> GetEventInvitesViewModel(AdminServiceRequest request, Guid eventId);

    Task<EventsAdminPageViewModel> GetEventsDto(AdminServiceRequest request, int page, int pageSize);    

    Task<EventSettingsAdminPageViewModel> GetEventSettingsViewModel(AdminServiceRequest request);
    
    Task<EventTicketsAdminPageViewModel> GetEventTicketsViewModel(AdminServiceRequest request, Guid eventId);

    Task<DateTime> GetNextAvailableEventDate(AdminServiceRequest request);

    Task PublishEvent(AdminServiceRequest request, Guid eventId);

    Task SendEventInviteeEmail(AdminServiceRequest request, Guid eventId, 
        IEnumerable<EventResponseType> responseTypes, string subject, string body);

    Task<ServiceResult> SendEventInvites(AdminServiceRequest request, Guid eventId, bool test = false);

    Task SendScheduledEmails();

    Task UpdateEventSettings(AdminServiceRequest request, UpdateEventSettings model);

    Task<ServiceResult> UpdateEvent(AdminServiceRequest request, Guid id, CreateEvent @event);

    Task UpdateMemberResponse(AdminServiceRequest request, Guid eventId, Guid memberId, 
        EventResponseType responseType);

    Task<ServiceResult> UpdateScheduledEmail(AdminServiceRequest request, Guid eventId, DateTime? date);
}
