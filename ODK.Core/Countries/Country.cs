namespace ODK.Core.Countries;

public class Country : IDatabaseEntity
{
    private static readonly IReadOnlyCollection<string> MilesCountries = ["US", "GB", "LR", "MM"];

    public required string Continent { get; set; }

    public Guid CurrencyId { get; set; }

    /// <summary>
    /// The country's default formatting locale (a culture name, e.g. "en-GB"). Seeded by deriving from
    /// <see cref="IsoCode2"/> and overridable in data; when null the resolver derives it on the fly.
    /// </summary>
    public string? DefaultLocale { get; set; }

    public DistanceUnitType DistanceUnit => MilesCountries.Contains(IsoCode2.ToUpperInvariant())
        ? DistanceUnitType.Miles
        : DistanceUnitType.Kilometres;

    public Guid Id { get; set; }

    public required string IsoCode2 { get; set; }

    public required string IsoCode3 { get; set; }

    public required string Name { get; set; }
}