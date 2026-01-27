using ODK.Core.Events;
using ODK.Services.Events.ViewModels;

namespace ODK.Services.Events;

public interface IEventAdminService
{
    Task<ServiceResult> CreateEvent(MemberChapterServiceRequest request, CreateEvent createEvent, bool draft);

    Task DeleteEvent(MemberChapterServiceRequest request, Guid id);

    Task<Event> GetEvent(MemberChapterServiceRequest request, Guid id);

    Task<EventAttendeesAdminPageViewModel> GetEventAttendeesViewModel(MemberChapterServiceRequest request, Guid eventId);

    Task<EventCreateAdminPageViewModel> GetEventCreateViewModel(MemberChapterServiceRequest request);

    Task<EventEditAdminPageViewModel> GetEventEditViewModel(MemberChapterServiceRequest request, Guid eventId);

    Task<EventInvitesAdminPageViewModel> GetEventInvitesViewModel(MemberChapterServiceRequest request, Guid eventId);

    Task<EventsAdminPageViewModel> GetEventsDto(MemberChapterServiceRequest request, int page, int pageSize);

    Task<EventSettingsAdminPageViewModel> GetEventSettingsViewModel(MemberChapterServiceRequest request);

    Task<EventTicketsAdminPageViewModel> GetEventTicketsViewModel(MemberChapterServiceRequest request, Guid eventId);

    Task<DateTime> GetNextAvailableEventDate(MemberChapterServiceRequest request);

    Task PublishEvent(MemberChapterServiceRequest request, Guid eventId);

    Task SendEventInviteeEmail(MemberChapterServiceRequest request, Guid eventId,
        IEnumerable<EventResponseType> responseTypes, string subject, string body);

    Task<ServiceResult> SendEventInvites(MemberChapterServiceRequest request, Guid eventId, bool test = false);

    Task UpdateEventSettings(MemberChapterServiceRequest request, UpdateEventSettings model);

    Task<ServiceResult> UpdateEvent(MemberChapterServiceRequest request, Guid id, CreateEvent @event);

    Task<ServiceResult> UpdateMemberResponse(MemberChapterServiceRequest request, Guid eventId, Guid memberId,
        EventResponseType responseType);

    Task<ServiceResult> UpdateScheduledEmail(MemberChapterServiceRequest request, Guid eventId, DateTime? date);
}