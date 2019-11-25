using AutoMapper;
using ODK.Core.Events;

namespace ODK.Web.Api.Events
{
    public class EventsMappingProfile : Profile
    {
        public EventsMappingProfile()
        {
            CreateMap<Event, EventResponse>();

            CreateMap<EventMemberResponse, EventMemberResponseResponse>();
        }
    }
}
