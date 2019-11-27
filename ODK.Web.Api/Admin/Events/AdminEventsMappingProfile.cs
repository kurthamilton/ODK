using AutoMapper;
using ODK.Core.Mail;
using ODK.Services.Events;
using ODK.Web.Api.Admin.Events.Requests;
using ODK.Web.Api.Admin.Events.Responses;

namespace ODK.Web.Api.Admin.Events
{
    public class AdminEventsMappingProfile : Profile
    {
        public AdminEventsMappingProfile()
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
        }
    }
}
