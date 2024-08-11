namespace ODK.Core.Countries;

public class DistanceUnit : IDatabaseEntity
{
    public string Abbreviation { get; set; } = "";

    public Guid Id { get; set; }

    public double Metres { get; set; }

    public string Name { get; set; } = "";

    public int Order { get; set; }
}
