using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ODK.Core.Events
{
    public interface IEventRepository
    {
        Task<IReadOnlyCollection<Event>> GetEvents(Guid chapterId, DateTime after);
        IReadOnlyCollection<EventResponse> GetEventResponses(int eventId);
        IReadOnlyCollection<EventResponse> GetMemberResponses(int memberId, IEnumerable<int> eventIds);
        Task<IReadOnlyCollection<Event>> GetPublicEvents(Guid chapterId, DateTime after);
        void UpdateEventResponse(EventResponse eventResponse);
    }
}