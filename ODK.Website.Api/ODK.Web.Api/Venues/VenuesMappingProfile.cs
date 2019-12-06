using AutoMapper;
using ODK.Core.Venues;
using ODK.Web.Api.Venues.Responses;

namespace ODK.Web.Api.Venues
{
    public class VenuesMappingProfile : Profile
    {
        public VenuesMappingProfile()
        {
            CreateMap<Venue, VenueApiResponse>();
        }
    }
}
