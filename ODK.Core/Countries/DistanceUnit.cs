namespace ODK.Core.Countries;

public class DistanceUnit
{
    public required string Abbreviation { get; init; }

    public required double Metres { get; init; }

    public required string Name { get; init; }

    public required DistanceUnitType Type { get; init; }
}