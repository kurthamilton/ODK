using System.Web;
using ODK.Core.Countries;
using ODK.Core.Utils;
using ODK.Core.Web;
using ODK.Data.Core;
using ODK.Services.Geolocation;
using ODK.Services.Integrations.Geolocation.Models;
using ODK.Services.Logging;

namespace ODK.Services.Integrations.Geolocation;

public class GoogleGeolocationService : IGeolocationService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILoggingService _loggingService;
    private readonly GoogleGeolocationServiceSettings _settings;
    private readonly IUnitOfWork _unitOfWork;

    public GoogleGeolocationService(
        IUnitOfWork unitOfWork,
        ILoggingService loggingService,
        GoogleGeolocationServiceSettings settings,
        IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
        _loggingService = loggingService;
        _settings = settings;
        _unitOfWork = unitOfWork;
    }

    public async Task<Country?> GetCountryFromLocation(LatLong location)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();

            var apiKey = HttpUtility.UrlEncode(_settings.ApiKey);

            var url = UrlBuilder
                .Base("https://maps.googleapis.com")
                .Path("/maps/api/geocode/json")
                .Query("key", _settings.ApiKey)
                .Query("latlng", $"{location.Lat},{location.Long}")
                .Build();

            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonUtils.Deserialize<GeocodeResponse>(json);

            var countryComponent = result?
                .Results?
                .FirstOrDefault()?
                .AddressComponents?
                .FirstOrDefault(x => x.Types?.Contains("country", StringComparer.OrdinalIgnoreCase) == true);

            if (countryComponent == null || string.IsNullOrEmpty(countryComponent.ShortName))
            {
                return null;
            }

            return await _unitOfWork.CountryRepository
                .GetByIsoCode2(countryComponent.ShortName)
                .Run();
        }
        catch (Exception ex)
        {
            await _loggingService.Error($"Error retrieving region from lat/long from Google Places API", ex);
            return null;
        }        
    }
}
