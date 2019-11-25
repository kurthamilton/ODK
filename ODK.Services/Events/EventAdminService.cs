using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ODK.Core.Chapters;
using ODK.Core.Events;
using ODK.Services.Exceptions;

namespace ODK.Services.Events
{
    public class EventAdminService : OdkAdminServiceBase, IEventAdminService
    {
        private readonly IEventRepository _eventRepository;

        public EventAdminService(IEventRepository eventRepository, IChapterRepository chapterRepository)
            : base(chapterRepository)
        {
            _eventRepository = eventRepository;
        }

        public async Task<Event> CreateEvent(Guid memberId, CreateEvent createEvent)
        {
            await AssertMemberIsChapterAdmin(memberId, createEvent.ChapterId);

            Event @event = new Event(Guid.Empty, createEvent.ChapterId, createEvent.Name, createEvent.Date, createEvent.Location, createEvent.Time, null,
                createEvent.Address, createEvent.MapQuery, createEvent.Description, createEvent.IsPublic);

            ValidateEvent(@event);

            return await _eventRepository.CreateEvent(@event);
        }

        public async Task<IReadOnlyCollection<Event>> GetEvents(Guid currentMemberId, Guid chapterId)
        {
            await AssertMemberIsChapterAdmin(currentMemberId, chapterId);

            return await _eventRepository.GetEvents(chapterId, null);
        }

        private static void ValidateEvent(Event @event)
        {
            if (string.IsNullOrWhiteSpace(@event.Name) ||
                string.IsNullOrWhiteSpace(@event.Location) ||
                @event.Date == DateTime.MinValue)
            {
                throw new OdkServiceException("Some required fields are missing");
            }
        }
    }
}
