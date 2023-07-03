using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ODK.Core.Events;
using ODK.Services.Emails;
using ODK.Services.Events;
using ODK.Web.Api.Admin.Events.Requests;
using ODK.Web.Api.Admin.Events.Responses;
using ODK.Web.Common;
using ODK.Web.Common.Events.Responses;

namespace ODK.Web.Api.Admin.Events
{
    [Authorize]
    [ApiController]
    [Route("Admin/Events")]
    public class EventsController : OdkControllerBase
    {
        private readonly IEmailService _emailService;
        private readonly IEventAdminService _eventAdminService;
        private readonly IMapper _mapper;

        public EventsController(IEventAdminService eventAdminService, IMapper mapper, IEmailService emailService)
        {
            _emailService = emailService;
            _eventAdminService = eventAdminService;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<ActionResult<EventApiResponse>> Post([FromForm] CreateEventApiRequest request)
        {
            CreateEvent @event = _mapper.Map<CreateEvent>(request);
            Event created = await _eventAdminService.CreateEventOld(GetMemberId(), @event);
            EventApiResponse response = _mapper.Map<EventApiResponse>(created);
            return Created(response);
        }

        [HttpGet]
        public async Task<IEnumerable<EventApiResponse>> Get(Guid chapterId, int? page, int? pageSize)
        {
            IReadOnlyCollection<Event> events = await _eventAdminService.GetEvents(GetMemberId(), chapterId, page ?? 1, pageSize ?? 10);
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

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvent(Guid id)
        {
            await _eventAdminService.DeleteEvent(GetMemberId(), id);
            return Ok();
        }

        [HttpPost("{id}/Invitees/SendEmail")]
        public async Task<IActionResult> SendEventInviteeEmail(Guid id, [FromForm] SendEventInviteeEmailApiRequest request)
        {
            await _eventAdminService.SendEventInviteeEmail(GetMemberId(), id, request.Statuses?.ToArray() ?? new EventResponseType[0],
                request.Subject, request.Body);
            return Created();
        }

        [HttpGet("{id}/Invites")]
        public async Task<EventInvitesApiResponse> EventInvites(Guid id)
        {
            EventInvites invites = await _eventAdminService.GetEventInvites(GetMemberId(), id);
            return _mapper.Map<EventInvitesApiResponse>(invites);
        }

        [HttpPost("{id}/Invites/Send")]
        public async Task<IActionResult> SendEventInvites(Guid id)
        {
            await _eventAdminService.SendEventInvites(GetMemberId(), id);
            return Created();
        }

        [HttpPost("{id}/Invites/Send/Test")]
        public async Task<IActionResult> SendTestEventInvites(Guid id)
        {
            await _eventAdminService.SendEventInvites(GetMemberId(), id, true);
            return Created();
        }

        [HttpGet("{id}/Responses")]
        public async Task<IEnumerable<EventResponseApiResponse>> EventResponses(Guid id)
        {
            IReadOnlyCollection<EventResponse> responses = await _eventAdminService.GetEventResponses(GetMemberId(), id);
            return responses.Select(_mapper.Map<EventResponseApiResponse>);
        }

        [HttpPut("{id}/Responses/{memberId}")]
        public async Task<EventResponseApiResponse> UpdateEventResponse(Guid id, Guid memberId,
            [FromForm] UpdateMemberResponseApiRequest request)
        {
            EventResponse response = await _eventAdminService.UpdateMemberResponse(GetMemberId(), id, memberId, request.Type);
            return _mapper.Map<EventResponseApiResponse>(response);
        }

        [HttpGet("Count")]
        public async Task<int> GetEventCount(Guid chapterId)
        {
            return await _eventAdminService.GetEventCount(GetMemberId(), chapterId);
        }

        [HttpGet("Invites")]
        public async Task<IEnumerable<EventInvitesApiResponse>> Invites(Guid chapterId, int? page, int? pageSize)
        {
            IReadOnlyCollection<EventInvites> invites = await _eventAdminService.GetChapterInvites(GetMemberId(), chapterId, page ?? 1, pageSize ?? 10);
            return invites.Select(_mapper.Map<EventInvitesApiResponse>);
        }

        [HttpGet("Responses/Chapters/{chapterId}")]
        public async Task<IEnumerable<EventResponseApiResponse>> ChapterResponses(Guid chapterId)
        {
            IReadOnlyCollection<EventResponse> responses = await _eventAdminService.GetChapterResponses(GetMemberId(), chapterId);
            return responses.Select(_mapper.Map<EventResponseApiResponse>);
        }

        [HttpGet("Responses/Members/{memberId}")]
        public async Task<IEnumerable<EventResponseApiResponse>> MemberResponses(Guid memberId)
        {
            IReadOnlyCollection<EventResponse> responses = await _eventAdminService.GetMemberResponses(GetMemberId(), memberId);
            return responses.Select(_mapper.Map<EventResponseApiResponse>);
        }

        [HttpGet("Venues/{id}")]
        public async Task<IEnumerable<EventApiResponse>> GetByVenue(Guid id)
        {
            IReadOnlyCollection<Event> events = await _eventAdminService.GetEventsByVenue(GetMemberId(), id);
            return events.Select(_mapper.Map<EventApiResponse>);
        }
    }
}
