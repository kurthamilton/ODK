using System.Net;
using ODK.Core.Countries;
using ODK.Core.Utils;
using ODK.Core.Web;
using ODK.Services.Integrations.Places.Models;
using ODK.Services.Logging;
using ODK.Services.Places;

namespace ODK.Services.Integrations.Places;

public class PlacesService : IPlacesService
{
    /* The fields a venue record is built from. displayName makes this a Pro-SKU request where the rest
       alone would be Essentials; the volume is a handful of lookups a day, and the name is the point. */
    private const string FieldMask = "id,displayName,location,addressComponents,formattedAddress";

    /* The address component holding a town or city. postal_town appears in countries - the UK among them -
       where locality is a village or suburb and the postal town is the name people would recognise, so it
       is preferred where both are present. */
    private static readonly string[] LocalityTypes = ["postal_town", "locality"];

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILoggingService _loggingService;
    private readonly PlacesServiceSettings _settings;

    public PlacesService(
        PlacesServiceSettings settings,
        IHttpClientFactory httpClientFactory,
        ILoggingService loggingService)
    {
        _httpClientFactory = httpClientFactory;
        _loggingService = loggingService;
        _settings = settings;
    }

    public async Task<GetPlaceResult> GetPlace(string externalId)
    {
        if (string.IsNullOrEmpty(externalId))
        {
            return GetPlaceResult.Failure("Place not specified");
        }

        if (string.IsNullOrEmpty(_settings.ApiKey))
        {
            await _loggingService.Error("Places lookup attempted with no API key configured");
            return GetPlaceResult.Failure("Place lookup is not available");
        }

        try
        {
            var client = _httpClientFactory.CreateClient();

            var url = UrlBuilder
                .Base("https://places.googleapis.com")
                .Path($"/v1/places/{Uri.EscapeDataString(externalId)}")
                .Build();

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("X-Goog-Api-Key", _settings.ApiKey);
            request.Headers.Add("X-Goog-FieldMask", FieldMask);

            var response = await client.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            /* A place that has closed, moved or been merged loses its ID, and the one we hold stops
               resolving. The member can put that right by picking the place again, so it is not an error
               to log. */
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return GetPlaceResult.PlaceNotFound();
            }

            if (!response.IsSuccessStatusCode)
            {
                await _loggingService.Error(
                    $"Error retrieving place '{externalId}' from Google Places API: {json}");
                return GetPlaceResult.Failure("Place lookup failed");
            }

            var place = JsonUtils.Deserialize<PlaceResponse>(json);

            /* A response missing an id, a name or a position is not a place we can record, and taking it
               as one would put a venue at the equator. */
            var name = place?.DisplayName?.Text;
            if (place?.Id == null || string.IsNullOrEmpty(name) || place.Location == null)
            {
                await _loggingService.Error(
                    $"Incomplete place '{externalId}' returned by Google Places API: {json}");
                return GetPlaceResult.Failure("Place lookup failed");
            }

            return GetPlaceResult.Found(new Place
            {
                ExternalId = place.Id,
                FormattedAddress = place.FormattedAddress,
                Location = new LatLong(place.Location.Latitude, place.Location.Longitude),
                Locality = GetLocality(place.AddressComponents),
                Name = name
            });
        }
        catch (Exception ex)
        {
            await _loggingService.Error(
                $"Error retrieving place '{externalId}' from Google Places API", ex);
            return GetPlaceResult.Failure("Place lookup failed");
        }
    }

    private static string? GetLocality(PlaceAddressComponentResponse[]? components)
    {
        if (components == null)
        {
            return null;
        }

        foreach (var type in LocalityTypes)
        {
            var component = components.FirstOrDefault(
                x => x.Types?.Contains(type, StringComparer.OrdinalIgnoreCase) == true);
            if (!string.IsNullOrEmpty(component?.LongText))
            {
                return component.LongText;
            }
        }

        return null;
    }
}
