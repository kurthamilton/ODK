namespace ODK.Web.Common.Config.Settings;

public class GoogleSettings
{
    public required GoogleGeolocationSettings Geolocation { get; init; }

    public required GoogleMapsSettings Maps { get; init; }
}