using AutoMapper;
using ODK.Core.Countries;
using ODK.Web.Common.Countries.Responses;

namespace ODK.Web.Common.Countries
{
    public class CountriesMappingProfile : Profile
    {
        public CountriesMappingProfile()
        {
            CreateMap<Country, CountryApiResponse>();
        }
    }
}
