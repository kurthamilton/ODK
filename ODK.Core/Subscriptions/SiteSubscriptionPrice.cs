namespace ODK.Core.Subscriptions;

public class SiteSubscriptionPrice : IDatabaseEntity
{
    public double Amount { get; set; }

    public string CurrencyCode { get; set; } = "";

    public string CurrencySymbol { get; set; } = "";

    public int Months { get; set; }

    public Guid Id { get; set; }

    public Guid SiteSubscriptionId { get; set; }
}
