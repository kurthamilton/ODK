using AutoMapper;
using ODK.Core.Emails;
using ODK.Core.Events;
using ODK.Services.Events;
using ODK.Web.Api.Admin.Events.Requests;
using ODK.Web.Api.Admin.Events.Responses;

namespace ODK.Web.Api.Admin.Events
{
    public class EventsMappingProfile : Profile
    {
        public EventsMappingProfile()
        {
            MapRequests();
            MapResponses();
        }

        private void MapRequests()
        {
            CreateMap<CreateEventApiRequest, CreateEvent>();
        }

        private void MapResponses()
        {
            CreateMap<Email, EventEmailApiResponse>();

            CreateMap<EventInvites, EventInvitesApiResponse>();
        }
    }
}
