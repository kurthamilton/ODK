using ODK.Core.Chapters;
using ODK.Core.Events;
using ODK.Services.Events.Models;
using ODK.Services.Events.ViewModels;

namespace ODK.Services.Events;

public interface IEventAdminService
{
    Task<ServiceResult> CreateEvent(MemberChapterAdminServiceRequest request, EventCreateModel createEvent, bool draft);

    Task DeleteEvent(MemberChapterAdminServiceRequest request, Guid id);

    Task<Event> GetEvent(MemberChapterAdminServiceRequest request, Guid id);

    Task<EventAttendeesAdminPageViewModel> GetEventAttendeesViewModel(MemberChapterAdminServiceRequest request, Guid eventId);

    Task<EventCommentsAdminPageViewModel> GetEventCommentsViewModel(MemberChapterAdminServiceRequest request, Guid eventId);

    Task<EventCreateAdminPageViewModel> GetEventCreateViewModel(MemberChapterAdminServiceRequest request);

    Task<EventEditAdminPageViewModel> GetEventEditViewModel(MemberChapterAdminServiceRequest request, Guid eventId);

    Task<EventInvitesAdminPageViewModel> GetEventInvitesViewModel(MemberChapterAdminServiceRequest request, Guid eventId);

    Task<EventsAdminPageViewModel> GetEventsAdminPageViewModel(MemberChapterAdminServiceRequest request, Chapter chapter, int page, int pageSize);

    Task<EventSettingsAdminPageViewModel> GetEventSettingsViewModel(MemberChapterAdminServiceRequest request);

    Task<EventTicketsAdminPageViewModel> GetEventTicketsViewModel(MemberChapterAdminServiceRequest request, Guid eventId);

    Task<DateTime> GetNextAvailableEventDate(MemberChapterAdminServiceRequest request);

    Task PublishEvent(MemberChapterAdminServiceRequest request, Guid eventId);

    Task<ServiceResult> SetEventCommentVisibility(
        MemberChapterAdminServiceRequest request,
        Guid eventId,
        Guid eventCommentId,
        bool hidden);

    Task SendEventInviteeEmail(MemberChapterAdminServiceRequest request, Guid eventId,
        IEnumerable<EventResponseType> responseTypes, string subject, string body);

    Task<ServiceResult> SendEventInvites(MemberChapterAdminServiceRequest request, Guid eventId, bool test = false);

    Task<ServiceResult> SetMissingEventShortcodes(MemberServiceRequest request);

    Task UpdateEventSettings(MemberChapterAdminServiceRequest request, EventSettingsUpdateModel model);

    Task<ServiceResult> UpdateEvent(MemberChapterAdminServiceRequest request, Guid id, EventCreateModel @event);

    Task<ServiceResult> UpdateMemberResponse(MemberChapterAdminServiceRequest request, Guid eventId, Guid memberId,
        EventResponseType responseType);

    Task<ServiceResult> UpdateScheduledEmail(MemberChapterAdminServiceRequest request, Guid eventId, DateTime? date);
}