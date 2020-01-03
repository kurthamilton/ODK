using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ODK.Core.Events;

namespace ODK.Services.Events
{
    public interface IEventAdminService
    {
        Task<Event> CreateEvent(Guid memberId, CreateEvent @event);

        Task DeleteEvent(Guid currentMemberId, Guid id);

        Task<IReadOnlyCollection<EventInvites>> GetChapterInvites(Guid currentMemberId, Guid chapterId, int page, int pageSize);

        Task<IReadOnlyCollection<EventResponse>> GetChapterResponses(Guid currentMemberId, Guid chapterId);

        Task<Event> GetEvent(Guid currentMemberId, Guid id);

        Task<int> GetEventCount(Guid currentMemberId, Guid chapterId);

        Task<EventInvites> GetEventInvites(Guid currentMemberId, Guid eventId);

        Task<IReadOnlyCollection<EventResponse>> GetEventResponses(Guid currentMemberId, Guid eventId);

        Task<IReadOnlyCollection<Event>> GetEvents(Guid currentMemberId, Guid chapterId, int page, int pageSize);

        Task<IReadOnlyCollection<Event>> GetEventsByVenue(Guid currentMemberId, Guid venueId);

        Task<IReadOnlyCollection<EventResponse>> GetMemberResponses(Guid currentMemberId, Guid memberId);

        Task SendEventInviteeEmail(Guid currentMemberId, Guid eventId, IEnumerable<EventResponseType> responseTypes, 
            string subject, string body);

        Task SendEventInvites(Guid currentMemberId, Guid eventId, bool test = false);

        Task<Event> UpdateEvent(Guid memberId, Guid id, CreateEvent @event);
    }
}
