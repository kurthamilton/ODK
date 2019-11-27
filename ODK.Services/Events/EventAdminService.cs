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

        public async Task DeleteEvent(Guid currentMemberId, Guid id)
        {
            await AssertEventCanBeDeleted(currentMemberId, id);

            await _eventRepository.DeleteEvent(id);
        }

        public async Task<IReadOnlyCollection<EventMemberResponse>> GetChapterResponses(Guid currentMemberId, Guid chapterId)
        {
            await AssertMemberIsChapterAdmin(currentMemberId, chapterId);

            return await _eventRepository.GetChapterResponses(chapterId);
        }

        public async Task<Event> GetEvent(Guid currentMemberId, Guid id)
        {
            Event @event = await _eventRepository.GetEvent(id);
            if (@event == null)
            {
                throw new OdkNotFoundException();
            }

            await AssertMemberIsChapterAdmin(currentMemberId, @event.ChapterId);

            return @event;
        }

        public async Task<IReadOnlyCollection<Event>> GetEvents(Guid currentMemberId, Guid chapterId)
        {
            await AssertMemberIsChapterAdmin(currentMemberId, chapterId);

            return await _eventRepository.GetEvents(chapterId, null);
        }        

        public async Task<Event> UpdateEvent(Guid memberId, Guid id, CreateEvent @event)
        {
            Event update = await GetEvent(memberId, id);

            update.Update(@event.Address, @event.Date, @event.Description, null, @event.IsPublic, @event.Location,
                @event.MapQuery, @event.Name, @event.Time);

            ValidateEvent(update);

            await _eventRepository.UpdateEvent(@update);

            return update;
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

        private async Task AssertEventCanBeDeleted(Guid currentMemberId, Guid id)
        {
            Event @event = await GetEvent(currentMemberId, id);

            IReadOnlyCollection<EventMemberResponse> responses = await _eventRepository.GetEventResponses(@event.Id);
            if (responses.Count > 0)
            {
                throw new OdkServiceException("Events with responses cannot be deleted");
            }
        }
    }
}
