using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ODK.Core.Events;
using ODK.Core.Mail;

namespace ODK.Services.Events
{
    public interface IEventAdminService
    {
        Task<Event> CreateEvent(Guid memberId, CreateEvent @event);

        Task DeleteEvent(Guid currentMemberId, Guid id);

        Task<IReadOnlyCollection<EventMemberResponse>> GetChapterResponses(Guid currentMemberId, Guid chapterId);

        Task<Event> GetEvent(Guid currentMemberId, Guid id);

        Task<Email> GetEventEmail(Guid currentMemberId, Guid eventId);

        Task<IReadOnlyCollection<Event>> GetEvents(Guid currentMemberId, Guid chapterId);

        Task<Event> UpdateEvent(Guid memberId, Guid id, CreateEvent @event);
    }
}
