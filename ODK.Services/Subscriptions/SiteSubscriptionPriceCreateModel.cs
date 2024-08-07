namespace ODK.Services.Subscriptions;

public class SiteSubscriptionPriceCreateModel
{
    public string CurrencyCode { get; set; } = "";

    public string CurrencySymbol { get; set; } = "";

    public double Amount { get; set; }

    public int Months { get; set; }
}
