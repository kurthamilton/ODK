using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ODK.Services.Countries;
using ODK.Web.Api.Countries.Responses;

namespace ODK.Web.Api.Countries
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class CountriesController : OdkControllerBase
    {
        private readonly ICountryService _countryService;
        private readonly IMapper _mapper;

        public CountriesController(ICountryService countryService, IMapper mapper)
        {
            _countryService = countryService;
            _mapper = mapper;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CountryApiResponse>>> Get()
        {
            return await HandleVersionedRequest(
                _countryService.GetCountries,
                x => x.Select(_mapper.Map<CountryApiResponse>));
        }
    }
}
