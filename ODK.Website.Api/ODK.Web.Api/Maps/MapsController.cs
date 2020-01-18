using System;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ODK.Core.Venues;
using ODK.Services;
using ODK.Services.Exceptions;
using ODK.Services.Settings;
using ODK.Services.Venues;
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
        private readonly IVenueService _venueService;

        public MapsController(ISettingsService settingsService, IMapper mapper, IVenueService venueService)
        {
            _mapper = mapper;
            _settingsService = settingsService;
            _venueService = venueService;
        }

        [AllowAnonymous]
        [HttpGet("Google/ApiKey")]
        public async Task<ActionResult<GoogleMapsApiKeyApiResponse>> GoogleMapsApiKey(Guid? venueId)
        {
            if (TryGetMemberId() == null)
            {
                VersionedServiceResult<Venue> venue = venueId != null
                    ? await _venueService.GetVenue(null, null, venueId.Value)
                    : new VersionedServiceResult<Venue>(0, null);

                if (venue.Value == null)
                {
                    throw new OdkNotAuthorizedException();
                }
            }

            return await HandleVersionedRequest(
                _settingsService.GetSiteSettings,
                _mapper.Map<GoogleMapsApiKeyApiResponse>);
        }
    }
}
