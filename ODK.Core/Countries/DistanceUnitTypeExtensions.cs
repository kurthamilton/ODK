namespace ODK.Core.Countries;

public static class DistanceUnitTypeExtensions
{
    public static double Metres(this DistanceUnitType unit) => unit switch
    {
        DistanceUnitType.Kilometres => 1000,
        DistanceUnitType.Miles => 1609.34,
        _ => throw new ArgumentOutOfRangeException(nameof(unit), $"Unsupported distance unit: {unit}")
    };
}