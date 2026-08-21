using ODK.Core.Countries;
using ODK.Data.Core.Countries;

namespace ODK.Services.Members.ViewModels;

public class LocationDefaultsViewModel
{
    public required Country? Country { get; init; }

    public required CurrencyDto? Currency { get; init; }

    public required DistanceUnit? DistanceUnit { get; init; }

    public required TimeZoneInfo? TimeZone { get; init; }
}