using ODK.Core.Chapters;
using ODK.Core.Events;
using ODK.Services.Events.Models;
using ODK.Services.Events.ViewModels;

namespace ODK.Services.Events;

public interface IEventAdminService
{
    Task<ServiceResult> CreateEvent(IMemberChapterAdminServiceRequest request, EventCreateModel createEvent, bool draft);

    Task DeleteEvent(IMemberChapterAdminServiceRequest request, Guid id);

    Task<Event> GetEvent(IMemberChapterAdminServiceRequest request, Guid id);

    Task<EventAttendeesAdminPageViewModel> GetEventAttendeesViewModel(IMemberChapterAdminServiceRequest request, Guid eventId);

    Task<EventCommentsAdminPageViewModel> GetEventCommentsViewModel(IMemberChapterAdminServiceRequest request, Guid eventId);

    Task<EventCreateAdminPageViewModel> GetEventCreateViewModel(IMemberChapterAdminServiceRequest request);

    Task<EventEditAdminPageViewModel> GetEventEditViewModel(IMemberChapterAdminServiceRequest request, Guid eventId);

    Task<EventInvitesAdminPageViewModel> GetEventInvitesViewModel(IMemberChapterAdminServiceRequest request, Guid eventId);

    Task<EventsAdminPageViewModel> GetEventsAdminPageViewModel(IMemberChapterAdminServiceRequest request, Chapter chapter, int page, int pageSize);

    Task<EventSettingsAdminPageViewModel> GetEventSettingsViewModel(IMemberChapterAdminServiceRequest request);

    Task<EventTicketsAdminPageViewModel> GetEventTicketsViewModel(IMemberChapterAdminServiceRequest request, Guid eventId);

    Task<DateTime> GetNextAvailableEventDate(IMemberChapterAdminServiceRequest request);

    Task PublishEvent(IMemberChapterAdminServiceRequest request, Guid eventId);

    Task<ServiceResult> SetEventCommentVisibility(
        IMemberChapterAdminServiceRequest request,
        Guid eventId,
        Guid eventCommentId,
        bool hidden);

    Task SendEventInviteeEmail(IMemberChapterAdminServiceRequest request, Guid eventId,
        IEnumerable<EventResponseType> responseTypes, string subject, string body);

    Task<ServiceResult> SendEventInvites(IMemberChapterAdminServiceRequest request, Guid eventId, bool test = false);

    Task<ServiceResult> SetMissingEventShortcodes(IMemberServiceRequest request);

    Task UpdateEventSettings(IMemberChapterAdminServiceRequest request, EventSettingsUpdateModel model);

    Task<ServiceResult> UpdateEvent(IMemberChapterAdminServiceRequest request, Guid id, EventCreateModel @event);

    Task<ServiceResult> UpdateMemberResponse(IMemberChapterAdminServiceRequest request, Guid eventId, Guid memberId,
        EventResponseType responseType);

    Task<ServiceResult> UpdateScheduledEmail(IMemberChapterAdminServiceRequest request, Guid eventId, DateTime? date);
}