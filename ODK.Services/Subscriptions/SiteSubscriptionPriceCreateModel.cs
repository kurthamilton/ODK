namespace ODK.Services.Subscriptions;

public class SiteSubscriptionPriceCreateModel
{    
    public Guid CurrencyId { get; set; }

    public int MonthlyAmount { get; set; }

    public int YearlyAmount { get; set; }
}
