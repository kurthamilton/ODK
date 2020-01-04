using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ODK.Core.Events;

namespace ODK.Services.Events
{
    public interface IEventService
    {
        Task<IReadOnlyCollection<EventResponse>> GetEventResponses(Guid currentMemberId, Guid eventId);

        Task<IReadOnlyCollection<Event>> GetEvents(Guid currentMemberId, Guid chapterId);

        Task<IReadOnlyCollection<EventResponse>> GetMemberResponses(Guid memberId);

        Task<IReadOnlyCollection<Event>> GetPublicEvents(Guid chapterId);

        Task<EventResponse> UpdateMemberResponse(Guid memberId, Guid eventId, EventResponseType responseType);
    }
}
