using AutoMapper;
using ODK.Core.Settings;
using ODK.Web.Common.Maps.Responses;

namespace ODK.Web.Common.Maps
{
    public class MapsMappingProfile : Profile
    {
        public MapsMappingProfile()
        {
            CreateMap<SiteSettings, GoogleMapsApiKeyApiResponse>()
                .ForMember(x => x.ApiKey, opt => opt.MapFrom(x => x.GoogleMapsApiKey));
        }
    }
}
