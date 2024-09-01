namespace ODK.Core.Countries;

public class Currency : IDatabaseEntity
{
    public string Code { get; set; } = "";

    public string Description { get; set; } = "";

    public Guid Id { get; set; }

    public string Symbol { get; set; } = "";

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

    public string ToAmountString(double amount) => ToAmountString((decimal)amount);

    public override string ToString() => $"{Symbol} {Code}";
}
