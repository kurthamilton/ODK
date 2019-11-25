using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ODK.Core.Events;
using ODK.Core.Members;
using ODK.Services.Exceptions;

namespace ODK.Services.Events
{
    public class EventService : IEventService
    {
        private readonly IEventRepository _eventRepository;
        private readonly IMemberRepository _memberRepository;

        public EventService(IEventRepository eventRepository, IMemberRepository memberRepository)
        {
            _eventRepository = eventRepository;
            _memberRepository = memberRepository;
        }

        public async Task CreateEvent(Guid currentMemberId, CreateEvent @event)
        {

        }

        public async Task<IReadOnlyCollection<EventMemberResponse>> GetEventResponses(Guid currentMemberId, Guid eventId)
        {
            await AssertMemberEventPermission(currentMemberId, eventId);

            return await _eventRepository.GetEventResponses(eventId);
        }

        public async Task<IReadOnlyCollection<Event>> GetEvents(Guid currentMemberId, Guid chapterId)
        {
            await AssertMemberChapterPermission(currentMemberId, chapterId);
            return await _eventRepository.GetEvents(chapterId, DateTime.Today);
        }

        public async Task<IReadOnlyCollection<Event>> GetPublicEvents(Guid chapterId)
        {
            return await _eventRepository.GetPublicEvents(chapterId, DateTime.Today);
        }

        public async Task UpdateMemberResponse(Guid memberId, Guid eventId, EventResponseType responseType)
        {
            responseType = NormalizeResponseType(responseType);

            Event @event = await GetEvent(memberId, eventId);
            if (@event.Date < DateTime.Today)
            {
                throw new OdkServiceException("Past events cannot be responded to");
            }

            EventMemberResponse response = new EventMemberResponse(eventId, memberId, responseType);
            await _eventRepository.UpdateEventResponse(response);
        }

        private static EventResponseType NormalizeResponseType(EventResponseType responseType)
        {
            if (!Enum.IsDefined(typeof(EventResponseType), responseType) || responseType == EventResponseType.None)
            {
                responseType = EventResponseType.No;
            }

            return responseType;
        }

        private async Task AssertMemberChapterPermission(Guid memberId, Guid chapterId)
        {
            Member member = await _memberRepository.GetMember(memberId);
            if (member?.ChapterId != chapterId)
            {
                throw new OdkNotAuthorizedException();
            }
        }

        private async Task AssertMemberChapterAdminPermission(Guid memberId, Guid chapterId)
        {
            Member member = await _memberRepository.GetMember(memberId);
            if (member?.ChapterId != chapterId)
            {
                throw new OdkNotAuthorizedException();
            }
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

            await AssertMemberChapterPermission(memberId, @event.ChapterId);
        }

        private async Task<Event> GetEvent(Guid currentMemberId, Guid eventId)
        {
            Event @event = await _eventRepository.GetEvent(eventId);
            await AssertMemberEventPermission(currentMemberId, @event);
            return @event;
        }
    }
}
