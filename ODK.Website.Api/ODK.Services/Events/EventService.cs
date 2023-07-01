using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ODK.Core.Events;
using ODK.Core.Members;
using ODK.Core.Venues;
using ODK.Services.Authorization;
using ODK.Services.Exceptions;

namespace ODK.Services.Events
{
    public class EventService : IEventService
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IEventRepository _eventRepository;
        private readonly IVenueRepository _venueRepository;

        public EventService(IEventRepository eventRepository, IAuthorizationService authorizationService,
            IVenueRepository venueRepository)
        {
            _authorizationService = authorizationService;
            _eventRepository = eventRepository;
            _venueRepository = venueRepository;
        }

        public async Task<Event> GetEvent(Guid chapterId, Guid eventId)
        {
            Event @event = await _eventRepository.GetEvent(eventId);
            if (@event == null || @event.ChapterId != chapterId)
            {
                return null;
            }

            return @event;
        }

        public async Task<IReadOnlyCollection<EventResponse>> GetEventResponses(Guid currentMemberId, Guid eventId)
        {
            await AssertMemberEventPermission(currentMemberId, eventId);
            return await GetEventResponses(eventId);
        }

        public async Task<IReadOnlyCollection<EventResponse>> GetEventResponses(Guid eventId)
        {
            return await _eventRepository.GetEventResponses(eventId);
        }

        public async Task<IReadOnlyCollection<EventResponseViewModel>> GetEventResponseViewModels(Member member, Guid chapterId)
        {
            IReadOnlyCollection<Event> events;
            if (member == null || member.ChapterId != chapterId)
            {
                events = await _eventRepository.GetPublicEvents(chapterId, DateTime.UtcNow.Date);
            }
            else
            {
                events = await _eventRepository.GetEvents(chapterId, DateTime.UtcNow.Date);
            }

            IReadOnlyCollection<EventResponse> responses = Array.Empty<EventResponse>();
            if (member != null)
            {
                responses = await _eventRepository.GetMemberResponses(member.Id);
            }

            IDictionary<Guid, EventResponseType> responseLookup = responses.ToDictionary(x => x.EventId, x => x.ResponseTypeId);

            IReadOnlyCollection<Venue> venues = await _venueRepository.GetVenues(chapterId);
            IDictionary<Guid, Venue> venueLookup = venues.ToDictionary(x => x.Id);

            List<EventResponseViewModel> viewModels = new List<EventResponseViewModel>();
            foreach (Event @event in events)
            {
                responseLookup.TryGetValue(@event.Id, out EventResponseType responseType);
                EventResponseViewModel viewModel = new EventResponseViewModel(@event, venueLookup[@event.VenueId], responseType);
                viewModels.Add(viewModel);
            }

            return viewModels;
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

            Event @event = await GetEventForMember(memberId, eventId);
            if (@event.Date < DateTime.Today)
            {
                throw new OdkServiceException("Past events cannot be responded to");
            }

            EventResponse response = new EventResponse(eventId, memberId, responseType);
            await _eventRepository.UpdateEventResponse(response);

            return new EventResponse(eventId, memberId, responseType);
        }

        public async Task UpdateMemberResponse(Member member, Guid eventId,
            EventResponseType responseType)
        {
            responseType = NormalizeResponseType(responseType);

            Event @event = await _eventRepository.GetEvent(eventId);
            if (@event == null || @event.Date < DateTime.Today)
            {
                return;
            }

            if (!@event.IsAuthorized(member))
            {
                return;
            }

            await _eventRepository.UpdateEventResponse(new EventResponse(eventId, member.Id, responseType));
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

        private async Task<Event> GetEventForMember(Guid currentMemberId, Guid eventId)
        {
            Event @event = await _eventRepository.GetEvent(eventId);
            await AssertMemberEventPermission(currentMemberId, @event);
            return @event;
        }
    }
}
