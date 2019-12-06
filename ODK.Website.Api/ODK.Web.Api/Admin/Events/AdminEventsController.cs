using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ODK.Core.Events;
using ODK.Core.Mail;
using ODK.Services.Events;
using ODK.Web.Api.Admin.Events.Requests;
using ODK.Web.Api.Admin.Events.Responses;
using ODK.Web.Api.Events.Responses;

namespace ODK.Web.Api.Admin.Events
{
    [Authorize]
    [ApiController]
    [Route("Admin/Events")]
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
        public async Task<EventApiResponse> UpdateEvent(Guid id, [FromForm] CreateEventApiRequest request)
        {
            CreateEvent @event = _mapper.Map<CreateEvent>(request);
            Event updated = await _eventAdminService.UpdateEvent(GetMemberId(), id, @event);
            return _mapper.Map<EventApiResponse>(updated);
        }

        [HttpGet("{id}/email")]
        public async Task<EventEmailApiResponse> EventEmail(Guid id)
        {
            Email email = await _eventAdminService.GetEventEmail(GetMemberId(), id);
            return _mapper.Map<EventEmailApiResponse>(email);
        }

        [HttpGet("{id}/invites")]
        public async Task<EventInvitesApiResponse> EventInvites(Guid id)
        {
            EventInvites invites = await _eventAdminService.GetEventInvites(GetMemberId(), id);
            return _mapper.Map<EventInvitesApiResponse>(invites);
        }

        [HttpGet("{id}/responses")]
        public async Task<IEnumerable<EventMemberResponseApiResponse>> EventResponses(Guid id)
        {
            IReadOnlyCollection<EventMemberResponse> responses = await _eventAdminService.GetEventResponses(GetMemberId(), id);
            return responses.Select(_mapper.Map<EventMemberResponseApiResponse>);
        }

        [HttpPost("{id}/sendinvites")]
        public async Task<IActionResult> SendEventInvites(Guid id)
        {
            await _eventAdminService.SendEventInvites(GetMemberId(), id);
            return Created();
        }

        [HttpGet("invites")]
        public async Task<IEnumerable<EventInvitesApiResponse>> Invites(Guid chapterId)
        {
            IReadOnlyCollection<EventInvites> invites = await _eventAdminService.GetChapterInvites(GetMemberId(), chapterId);
            return invites.Select(_mapper.Map<EventInvitesApiResponse>);
        }

        [HttpGet("responses")]
        public async Task<IEnumerable<EventMemberResponseApiResponse>> ChapterResponses(Guid chapterId)
        {
            IReadOnlyCollection<EventMemberResponse> responses = await _eventAdminService.GetChapterResponses(GetMemberId(), chapterId);
            return responses.Select(_mapper.Map<EventMemberResponseApiResponse>);
        }
    }
}
