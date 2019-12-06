using AutoMapper;
using ODK.Core.Settings;
using ODK.Web.Api.Maps.Responses;

namespace ODK.Web.Api.Maps
{
    public class MapsProfile : Profile
    {
        public MapsProfile()
        {
            CreateMap<SiteSettings, GoogleMapsApiKeyApiResponse>()
                .ForMember(x => x.ApiKey, opt => opt.MapFrom(x => x.GoogleMapsApiKey));
        }
    }
}
