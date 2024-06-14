using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ODK.Core.Events;

namespace ODK.Services.Events
{
    public interface IEventAdminService
    {
        Task<ServiceResult> CreateEvent(Guid memberId, CreateEvent createEvent);

        Task DeleteEvent(Guid currentMemberId, Guid id);
        
        Task<IReadOnlyCollection<EventInvites>> GetChapterInvites(Guid currentMemberId, Guid chapterId, IReadOnlyCollection<Guid> eventIds);
        
        Task<IReadOnlyCollection<EventResponse>> GetChapterResponses(Guid currentMemberId, Guid chapterId, IReadOnlyCollection<Guid> eventIds);

        Task<Event> GetEvent(Guid currentMemberId, Guid id);
        
        Task<EventInvites> GetEventInvites(Guid currentMemberId, Guid eventId);

        Task<IReadOnlyCollection<EventResponse>> GetEventResponses(Guid currentMemberId, Guid eventId);

        Task<IReadOnlyCollection<Event>> GetEvents(Guid currentMemberId, Guid chapterId, int page, int pageSize);

        Task<IReadOnlyCollection<Event>> GetEventsByVenue(Guid currentMemberId, Guid venueId);
        
        Task SendEventInviteeEmail(Guid currentMemberId, Guid eventId, IEnumerable<EventResponseType> responseTypes, 
            string subject, string body);

        Task<ServiceResult> SendEventInvites(Guid currentMemberId, Guid eventId, bool test = false);

        Task<ServiceResult> UpdateEvent(Guid memberId, Guid id, CreateEvent @event);

        Task<Event> UpdateEventOld(Guid memberId, Guid id, CreateEvent @event);

        Task<EventResponse> UpdateMemberResponse(Guid currentMemberId, Guid eventId, Guid memberId, EventResponseType responseType);
    }
}
