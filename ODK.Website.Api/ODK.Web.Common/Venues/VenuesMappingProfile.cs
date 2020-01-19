using AutoMapper;
using ODK.Core.Venues;
using ODK.Web.Common.Venues.Responses;

namespace ODK.Web.Common.Venues
{
    public class VenuesMappingProfile : Profile
    {
        public VenuesMappingProfile()
        {
            CreateMap<Venue, VenueApiResponse>();
        }
    }
}
