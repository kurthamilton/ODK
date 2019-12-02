using AutoMapper;
using ODK.Core.Countries;
using ODK.Web.Api.Countries.Responses;

namespace ODK.Web.Api.Countries
{
    public class CountriesMappingProfile : Profile
    {
        public CountriesMappingProfile()
        {
            CreateMap<Country, CountryApiResponse>();
        }
    }
}
