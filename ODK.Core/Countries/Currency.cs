namespace ODK.Core.Countries;

public class Currency : IDatabaseEntity
{
    public string Code { get; set; } = "";

    public string Description { get; set; } = "";

    public Guid Id { get; set; }

    public string Symbol { get; set; } = "";

    public string ToAmountString(decimal amount)
    {
        var intAmount = (int)amount;
        return intAmount == amount
            ? $"{Symbol}{intAmount}"
            : $"{Symbol}{amount:0.00}";
    }

    public string ToAmountString(double amount) => ToAmountString((decimal)amount);
}
