using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ODK.Core.Events;
using ODK.Services.Authorization;
using ODK.Services.Exceptions;

namespace ODK.Services.Events
{
    public class EventService : IEventService
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IEventRepository _eventRepository;

        public EventService(IEventRepository eventRepository, IAuthorizationService authorizationService)
        {
            _authorizationService = authorizationService;
            _eventRepository = eventRepository;
        }

        public async Task<IReadOnlyCollection<EventResponse>> GetEventResponses(Guid currentMemberId, Guid eventId)
        {
            await AssertMemberEventPermission(currentMemberId, eventId);
            return await _eventRepository.GetEventResponses(eventId);
        }

        public async Task<IReadOnlyCollection<Event>> GetEvents(Guid currentMemberId, Guid chapterId)
        {
            await _authorizationService.AssertMemberIsChapterMember(currentMemberId, chapterId);
            await _authorizationService.AssertMembershipIsActive(currentMemberId, chapterId);
            return await _eventRepository.GetEvents(chapterId, DateTime.UtcNow.Date);
        }

        public async Task<IReadOnlyCollection<EventResponse>> GetMemberResponses(Guid memberId)
        {
            return await _eventRepository.GetMemberResponses(memberId);
        }

        public async Task<IReadOnlyCollection<Event>> GetPublicEvents(Guid chapterId)
        {
            return await _eventRepository.GetPublicEvents(chapterId, DateTime.UtcNow.Date);
        }

        public async Task<EventResponse> UpdateMemberResponse(Guid memberId, Guid eventId, EventResponseType responseType)
        {
            responseType = NormalizeResponseType(responseType);

            Event @event = await GetEvent(memberId, eventId);
            if (@event.Date < DateTime.Today)
            {
                throw new OdkServiceException("Past events cannot be responded to");
            }

            EventResponse response = new EventResponse(eventId, memberId, responseType);
            await _eventRepository.UpdateEventResponse(response);

            return new EventResponse(eventId, memberId, responseType);
        }

        private static EventResponseType NormalizeResponseType(EventResponseType responseType)
        {
            if (!Enum.IsDefined(typeof(EventResponseType), responseType) || responseType <= EventResponseType.None)
            {
                responseType = EventResponseType.No;
            }

            return responseType;
        }

        private async Task AssertMemberEventPermission(Guid memberId, Guid eventId)
        {
            Event @event = await _eventRepository.GetEvent(eventId);
            await AssertMemberEventPermission(memberId, @event);
        }

        private async Task AssertMemberEventPermission(Guid memberId, Event @event)
        {
            if (@event == null)
            {
                throw new OdkNotFoundException();
            }

            if (@event.IsPublic)
            {
                return;
            }

            await _authorizationService.AssertMemberIsChapterMember(memberId, @event.ChapterId);
            await _authorizationService.AssertMembershipIsActive(memberId, @event.ChapterId);
        }

        private async Task<Event> GetEvent(Guid currentMemberId, Guid eventId)
        {
            Event @event = await _eventRepository.GetEvent(eventId);
            await AssertMemberEventPermission(currentMemberId, @event);
            return @event;
        }
    }
}
