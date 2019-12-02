using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ODK.Core.Events;
using ODK.Services.Events;
using ODK.Web.Api.Events.Responses;

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
        [HttpGet("public")]
        public async Task<IEnumerable<EventApiResponse>> Public(Guid chapterId)
        {
            IReadOnlyCollection<Event> events = await _eventService.GetPublicEvents(chapterId);
            return events.Select(_mapper.Map<EventApiResponse>);
        }

        [HttpPut("{id}/respond")]
        public async Task<EventMemberResponseApiResponse> Respond(Guid id, EventResponseType type)
        {
            EventMemberResponse response = await _eventService.UpdateMemberResponse(GetMemberId(), id, type);
            return _mapper.Map<EventMemberResponseApiResponse>(response);
        }

        [HttpGet("{id}/responses")]
        public async Task<IEnumerable<EventMemberResponseApiResponse>> Responses(Guid id)
        {
            IReadOnlyCollection<EventMemberResponse> responses = await _eventService.GetEventResponses(GetMemberId(), id);
            return responses.Select(_mapper.Map<EventMemberResponseApiResponse>);
        }
    }
}
