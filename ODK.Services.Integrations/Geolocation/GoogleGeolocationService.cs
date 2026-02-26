using System.Globalization;
using System.Web;
using GeoTimeZone;
using ODK.Core.Countries;
using ODK.Core.Utils;
using ODK.Core.Web;
using ODK.Data.Core;
using ODK.Services.Exceptions;
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
        var countryInfo = await GetCountryInfoFromLocation(location);
        if (countryInfo == null)
        {
            return null;
        }

        try
        {
            var country = await _unitOfWork.CountryRepository
                .GetByIsoCode2(countryInfo.IsoCode2)
                .Run();

            if (country != null)
            {
                return country;
            }

            return await CreateCountry(countryInfo);
        }
        catch (Exception ex)
        {
            await _loggingService.Error($"Error getting country for ISO code {countryInfo.IsoCode2}", ex);
            return null;
        }
    }

    public async Task<Location?> GetLocationFromIpAddress(string ipAddress)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();

            var url = $"http://ip-api.com/json/{ipAddress}";
            var response = await client.GetAsync(url);
            var responseJson = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                await _loggingService.Error($"ip-api error response: {responseJson}");
                return null;
            }

            if (!JsonUtils.TryDeserialize<IpApiResponse>(responseJson, out var ipApiResponse))
            {
                await _loggingService.Error($"Could not deserialize ip-api response: {responseJson}");
                return null;
            }

            if (ipApiResponse.Latitude == null || ipApiResponse.Longitude == null)
            {
                await _loggingService.Error($"Lat/Long not in ip-api response: {responseJson}");
                return null;
            }

            return new Location
            {
                LatLong = new LatLong(ipApiResponse.Latitude.Value, ipApiResponse.Longitude.Value),
                Name = $"{ipApiResponse.City}, {ipApiResponse.CountryCode}"
            };
        }
        catch (Exception ex)
        {
            await _loggingService.Error("Error calling ip-api", ex);
            return null;
        }
    }

    public Task<TimeZoneInfo?> GetTimeZoneFromLocation(LatLong location)
    {
        var ianaId = TimeZoneLookup.GetTimeZone(location.Lat, location.Long).Result;

        var timeZone = TimeZoneInfo.TryConvertIanaIdToWindowsId(ianaId, out var timeZoneId)
            ? TimeZoneInfo.FindSystemTimeZoneById(timeZoneId)
            : null;

        return Task.FromResult(timeZone);
    }

    private async Task<Country> CreateCountry(CountryInfo countryInfo)
    {
        var currencyInfo = await GetCurrencyInfoFromCountryCode(countryInfo.IsoCode2);
        if (currencyInfo == null)
        {
            throw new OdkServiceException($"Could not get currency info for country code '{countryInfo.IsoCode2}'");
        }

        var currency = await _unitOfWork.CurrencyRepository.GetByCode(currencyInfo.Code).Run();
        if (currency == null)
        {
            currency = new Currency
            {
                Code = currencyInfo.Code,
                Symbol = currencyInfo.Symbol,
            };

            _unitOfWork.CurrencyRepository.Add(currency);
        }

        var country = new Country
        {
            Continent = "UNKNOWN",
            CurrencyId = currency.Id,
            IsoCode2 = countryInfo.IsoCode2,
            IsoCode3 = countryInfo.IsoCode3,
            Name = countryInfo.Name
        };

        _unitOfWork.CountryRepository.Add(country);
        await _unitOfWork.SaveChangesAsync();

        return country;
    }

    private async Task<CountryInfo?> GetCountryInfoFromLocation(LatLong location)
    {
        var isoCode2 = await GetCountryIsoCode2FromLocation(location);
        if (isoCode2 == null)
        {
            return null;
        }

        try
        {
            var region = new RegionInfo(isoCode2);
            return new CountryInfo
            {
                IsoCode2 = region.TwoLetterISORegionName,
                IsoCode3 = region.ThreeLetterISORegionName,
                Name = region.EnglishName
            };
        }
        catch (Exception ex)
        {
            await _loggingService.Error($"Error getting RegionInfo from country ISO code {isoCode2}", ex);
            return null;
        }
    }

    private async Task<string?> GetCountryIsoCode2FromLocation(LatLong location)
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

            return countryComponent?.ShortName;
        }
        catch (Exception ex)
        {
            await _loggingService.Error($"Error retrieving region from lat/long from Google Places API", ex);
            return null;
        }
    }

    private Task<CurrencyInfo?> GetCurrencyInfoFromCountryCode(string isoCode2)
    {
        var region = new RegionInfo(isoCode2);

        var currencyInfo = new CurrencyInfo
        {
            Code = region.ISOCurrencySymbol,
            Symbol = region.CurrencySymbol
        };

        return Task.FromResult<CurrencyInfo?>(currencyInfo);
    }
}