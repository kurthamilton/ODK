namespace ODK.Core.Countries;

public class DistanceUnitFactory : IDistanceUnitFactory
{
    private static readonly IReadOnlyDictionary<DistanceUnitType, DistanceUnit> _distanceUnits = new[]
    {
        new DistanceUnit { Abbreviation = "km", Metres = 1000, Name = "Kilometres", Type = DistanceUnitType.Kilometres },
        new DistanceUnit { Abbreviation = "miles", Metres = 1609.34, Name = "Miles", Type = DistanceUnitType.Miles }
    }.ToDictionary(x => x.Type);

    public DistanceUnit Get(DistanceUnitType type) => _distanceUnits[type];

    public IReadOnlyCollection<DistanceUnit> GetAll() => _distanceUnits.Values.ToArray();

    public DistanceUnit GetByCountry(Country country) => country.IsoCode2.ToUpperInvariant() switch
    {
        "US" or "GB" or "LR" or "MM" => _distanceUnits[DistanceUnitType.Miles],
        _ => _distanceUnits[DistanceUnitType.Kilometres]
    };

    public DistanceUnit GetDefault() => _distanceUnits[DistanceUnitType.Miles];
}