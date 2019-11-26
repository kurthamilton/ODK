using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ODK.Core.Events;

namespace ODK.Services.Events
{
    public interface IEventAdminService
    {
        Task<Event> CreateEvent(Guid memberId, CreateEvent @event);

        Task<Event> GetEvent(Guid currentMemberId, Guid id);

        Task<IReadOnlyCollection<Event>> GetEvents(Guid currentMemberId, Guid chapterId);

        Task<Event> UpdateEvent(Guid memberId, Guid id, CreateEvent @event);
    }
}
