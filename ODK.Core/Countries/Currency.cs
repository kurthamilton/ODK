namespace ODK.Core.Countries;

public class Currency : IDatabaseEntity
{
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// The country whose flag and codes stand for the currency, for a currency that no country in
    /// <c>Countries</c> owns - so only the euro, which every eurozone country would otherwise claim. Null
    /// for the rest, whose country is the one referencing them.
    /// </summary>
    public string? CountryIsoCode2 { get; set; }

    /// <inheritdoc cref="CountryIsoCode2"/>
    public string? CountryIsoCode3 { get; set; }

    /// <inheritdoc cref="CountryIsoCode2"/>
    public string? CountryName { get; set; }

    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Symbol { get; set; } = string.Empty;

    public static string ToValueString(decimal amount)
    {
        var intAmount = (int)amount;
        return intAmount == amount
            ? intAmount.ToString()
            : amount.ToString("0.00");
    }

    public string ToAmountString(decimal amount)
    {
        var valueString = ToValueString(amount);
        return $"{Symbol}{valueString}";
    }

    public override string ToString() => $"{Symbol} {Code}";
}
