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
using ODK.Web.Api.Events;
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

        [HttpGet]
        public async Task<IEnumerable<EventApiResponse>> Get(Guid chapterId)
        {
            IReadOnlyCollection<Event> events = await _eventAdminService.GetEvents(GetMemberId(), chapterId);
            return events.Select(_mapper.Map<EventApiResponse>);
        }
    }
}
