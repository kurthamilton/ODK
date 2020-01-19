using AutoMapper;
using ODK.Core.Events;
using ODK.Web.Common.Events.Responses;

namespace ODK.Web.Common.Events
{
    public class EventsMappingProfile : Profile
    {
        public EventsMappingProfile()
        {
            CreateMap<Event, EventApiResponse>()
                .ForMember(x => x.IsPublic, opt => opt.Condition(x => x.IsPublic));

            CreateMap<EventResponse, EventResponseApiResponse>();
        }
    }
}
