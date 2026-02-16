namespace ODK.Core.Countries;

public class DistanceUnit : IDatabaseEntity
{
    public string Abbreviation { get; set; } = string.Empty;

    public Guid Id { get; set; }

    public double Metres { get; set; }

    public string Name { get; set; } = string.Empty;

    public int Order { get; set; }

    public DistanceUnitType Type { get; set; }
}