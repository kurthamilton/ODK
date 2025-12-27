namespace ODK.Core.Countries;

public class Country : IDatabaseEntity
{
    public required string Continent { get; set; }

    public Guid CurrencyId { get; set; }

    public Guid Id { get; set; }

    public required string IsoCode2 { get; set; }

    public required string IsoCode3 { get; set; }

    public required string Name { get; set; }
}
