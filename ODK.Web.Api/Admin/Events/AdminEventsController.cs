using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ODK.Core.Events;
using ODK.Services.Events;
using ODK.Web.Api.Admin.Events.Requests;
using ODK.Web.Api.Events.Responses;

namespace ODK.Web.Api.Admin.Events
{
    [Authorize]
    [ApiController]
    [Route("admin/events")]
    public class AdminEventsController : OdkControllerBase
    {
        private readonly IEventAdminService _eventAdminService;
        private readonly IMapper _mapper;

        public AdminEventsController(IEventAdminService eventAdminService, IMapper mapper)
        {
            _eventAdminService = eventAdminService;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<ActionResult<EventApiResponse>> Post([FromForm] CreateEventApiRequest request)
        {
            CreateEvent @event = _mapper.Map<CreateEvent>(request);
            Event created = await _eventAdminService.CreateEvent(GetMemberId(), @event);
            EventApiResponse response = _mapper.Map<EventApiResponse>(created);
            return Created(response);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvent(Guid id)
        {
            await _eventAdminService.DeleteEvent(GetMemberId(), id);
            return Ok();
        }

        [HttpGet]
        public async Task<IEnumerable<EventApiResponse>> Get(Guid chapterId)
        {
            IReadOnlyCollection<Event> events = await _eventAdminService.GetEvents(GetMemberId(), chapterId);
            return events.Select(_mapper.Map<EventApiResponse>);
        }

        [HttpGet("{id}")]
        public async Task<EventApiResponse> GetEvent(Guid id)
        {
            Event @event = await _eventAdminService.GetEvent(GetMemberId(), id);
            return _mapper.Map<EventApiResponse>(@event);
        }

        [HttpPut("{id}")]
        public async Task<EventApiResponse> UpdateEvent(Guid id, CreateEventApiRequest request)
        {
            CreateEvent @event = _mapper.Map<CreateEvent>(request);
            Event updated = await _eventAdminService.UpdateEvent(GetMemberId(), id, @event);
            return _mapper.Map<EventApiResponse>(updated);
        }
    }
}
