using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ODK.Core.Events;
using ODK.Services.Events;
using ODK.Web.Common;
using ODK.Web.Common.Events.Responses;

namespace ODK.Web.Api.Events
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class EventsController : OdkControllerBase
    {
        private readonly IEventService _eventService;
        private readonly IMapper _mapper;

        public EventsController(IEventService eventService, IMapper mapper)
        {
            _eventService = eventService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IEnumerable<EventApiResponse>> Get(Guid chapterId)
        {
            IReadOnlyCollection<Event> events = await _eventService.GetEvents(GetMemberId(), chapterId);
            return events.Select(_mapper.Map<EventApiResponse>);
        }

        [AllowAnonymous]
        [HttpGet("Public")]
        public async Task<IEnumerable<EventApiResponse>> Public(Guid chapterId)
        {
            IReadOnlyCollection<Event> events = await _eventService.GetPublicEvents(chapterId);
            return events.Select(_mapper.Map<EventApiResponse>);
        }

        [HttpPut("{id}/Respond")]
        public async Task<EventResponseApiResponse> Respond(Guid id, EventResponseType type)
        {
            EventResponse response = await _eventService.UpdateMemberResponseOld(GetMemberId(), id, type);
            return _mapper.Map<EventResponseApiResponse>(response);
        }

        [HttpGet("{id}/Responses")]
        public async Task<IEnumerable<EventResponseApiResponse>> Responses(Guid id)
        {
            IReadOnlyCollection<EventResponse> responses = await _eventService.GetEventResponses(GetMemberId(), id);
            return responses.Select(_mapper.Map<EventResponseApiResponse>);
        }

        [HttpGet("Responses")]
        public async Task<IEnumerable<EventResponseApiResponse>> MemberResponses()
        {
            IReadOnlyCollection<EventResponse> responses = await _eventService.GetMemberResponses(GetMemberId());
            return responses.Select(_mapper.Map<EventResponseApiResponse>);
        }
    }
}
