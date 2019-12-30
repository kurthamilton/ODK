using AutoMapper;
using ODK.Core.Events;
using ODK.Web.Api.Events.Responses;

namespace ODK.Web.Api.Events
{
    public class EventsMappingProfile : Profile
    {
        public EventsMappingProfile()
        {
            CreateMap<Event, EventApiResponse>()
                .ForMember(x => x.IsPublic, opt => opt.Condition(x => x.IsPublic));

            CreateMap<EventMemberResponse, EventMemberResponseApiResponse>();
        }
    }
}
