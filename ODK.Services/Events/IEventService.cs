using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ODK.Core.Events;

namespace ODK.Services.Events
{
    public interface IEventService
    {
        Task<IReadOnlyCollection<Event>> GetEvents(Guid memberId, Guid chapterId);

        Task<IReadOnlyCollection<Event>> GetPublicEvents(Guid chapterId);
    }
}
