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

        public async Task<IReadOnlyCollection<Event>> GetEvents(Guid memberId, Guid chapterId)
        {
            Member member = await _memberRepository.GetMember(memberId);

            if (member?.ChapterId != chapterId)
            {
                throw new OdkNotAuthorizedException();
            }

            return await _eventRepository.GetEvents(chapterId, DateTime.Today);
        }

        public async Task<IReadOnlyCollection<Event>> GetPublicEvents(Guid chapterId)
        {
            return await _eventRepository.GetPublicEvents(chapterId, DateTime.Today);
        }
    }
}
