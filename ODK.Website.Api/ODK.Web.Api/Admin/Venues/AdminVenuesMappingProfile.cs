using AutoMapper;
using ODK.Services.Venues;
using ODK.Web.Api.Admin.Venues.Requests;

namespace ODK.Web.Api.Admin.Venues
{
    public class AdminVenuesMappingProfile : Profile
    {
        public AdminVenuesMappingProfile()
        {
            CreateMap<CreateVenueApiRequest, CreateVenue>();
        }
    }
}
