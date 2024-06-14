namespace ODK.Core.Countries;

public class Country
{
    public Country(Guid id, string name, string continent, string currencyCode, string currencySymbol)
    {
        Continent = continent;
        CurrencyCode = currencyCode;
        CurrencySymbol = currencySymbol;
        Id = id;
        Name = name;
    }

    public string Continent { get; }

    public string CurrencyCode { get; }

    public string CurrencySymbol { get; }

    public Guid Id { get; }

    public string Name { get; }
}
