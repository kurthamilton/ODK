using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ODK.Services.Settings;
using ODK.Web.Api.Maps.Responses;

namespace ODK.Web.Api.Maps
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class MapsController : OdkControllerBase
    {
        private readonly IMapper _mapper;
        private readonly ISettingsService _settingsService;

        public MapsController(ISettingsService settingsService, IMapper mapper)
        {
            _mapper = mapper;
            _settingsService = settingsService;
        }

        [HttpGet("Google/ApiKey")]
        public async Task<ActionResult<GoogleMapsApiKeyApiResponse>> GoogleMapsApiKey()
        {
            return await HandleVersionedRequest(
                _settingsService.GetSiteSettings,
                _mapper.Map<GoogleMapsApiKeyApiResponse>);
        }
    }
}
