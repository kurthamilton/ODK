using AutoMapper;
using ODK.Services.Events;
using ODK.Web.Api.Admin.Events.Requests;

namespace ODK.Web.Api.Admin.Events
{
    public class AdminEventsMappingProfile : Profile
    {
        public AdminEventsMappingProfile()
        {
            CreateMap<CreateEventApiRequest, CreateEvent>();
        }
    }
}
