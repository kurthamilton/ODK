using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ODK.Core.Events;
using ODK.Services.Events;

namespace ODK.Web.Api.Events
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class EventsController : OdkControllerBase
    {
        private readonly IEventService _eventService;

        public EventsController(IEventService eventService)
        {
            _eventService = eventService;
        }

        [HttpGet]
        public async Task<IEnumerable<EventResponse>> Get(Guid chapterId)
        {
            IReadOnlyCollection<Event> events = await _eventService.GetEvents(GetMemberId(), chapterId);
            return events.Select(MapEventsResponseEvent);
        }

        [AllowAnonymous]
        [HttpGet("public")]
        public async Task<IEnumerable<EventResponse>> Public(Guid chapterId)
        {
            IReadOnlyCollection<Event> events = await _eventService.GetPublicEvents(chapterId);
            return events.Select(MapEventsResponseEvent);
        }

        private static EventResponse MapEventsResponseEvent(Event @event)
        {
            return new EventResponse
            {
                Address = @event.Address,
                ChapterId = @event.ChapterId,
                Date = @event.Date,
                Description = @event.Description,
                Id = @event.Id,
                ImageUrl = @event.ImageUrl,
                IsPublic = @event.IsPublic,
                Location = @event.Location,
                MapQuery = @event.MapQuery,
                Name = @event.Name,
                Time = @event.Time
            };
        }
    }
}
