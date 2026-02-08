using ODK.Core.Countries;

namespace ODK.Services.Geolocation;

public interface IGeolocationService
{
    Task<Country?> GetCountryFromLocation(LatLong location);

    Task<TimeZoneInfo?> GetTimeZoneFromLocation(LatLong location);
}