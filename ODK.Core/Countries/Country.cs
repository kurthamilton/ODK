namespace ODK.Core.Countries;

public class Country : IDatabaseEntity
{
    public string Continent { get; set; } = "";

    public Guid Id { get; set; }

    public string Name { get; set; } = "";
}
