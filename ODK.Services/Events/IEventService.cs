using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ODK.Core.Events;

namespace ODK.Services.Events
{
    public interface IEventService
    {
        Task CreateEvent(Guid currentMemberId, CreateEvent @event);

        Task<IReadOnlyCollection<EventMemberResponse>> GetEventResponses(Guid currentMemberId, Guid eventId);

        Task<IReadOnlyCollection<Event>> GetEvents(Guid currentMemberId, Guid chapterId);

        Task<IReadOnlyCollection<Event>> GetPublicEvents(Guid chapterId);

        Task UpdateMemberResponse(Guid memberId, Guid eventId, EventResponseType responseType);
    }
}
